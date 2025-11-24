#!/bin/bash

# TTS项目Debian特殊部署脚本
# 专门为Debian Trixie系统优化

set -e

# 配置变量
DEPLOY_PATH="/var/www/tts"
SERVICE_NAME="tts"
BACKUP_DIR="$DEPLOY_PATH/backups"
LOG_DIR="/var/log/tts"
NGINX_CONF="/etc/nginx/sites-available/tts"

# 确保脚本以root权限运行
if [ "$EUID" -ne 0 ]; then
  echo "请使用root权限运行此脚本"
  exit 1
fi

# 创建必要的目录
create_directories() {
    echo "创建必要的目录..."
    mkdir -p $DEPLOY_PATH
    mkdir -p $BACKUP_DIR
    mkdir -p $LOG_DIR
    mkdir -p /var/www/.dotnet
    
    # 设置权限
    chown -R www-data:www-data $DEPLOY_PATH
    chown -R www-data:www-data $LOG_DIR
    chown -R www-data:www-data /var/www/.dotnet
}

# 安装依赖
install_dependencies() {
    echo "检查系统依赖..."
    
    # 检查并安装基础系统依赖
    if ! dpkg -l | grep -q supervisor; then
        echo "安装supervisor..."
        apt-get install -y supervisor
    else
        echo "supervisor已安装"
    fi
    
    if ! dpkg -l | grep -q nginx; then
        echo "安装nginx..."
        apt-get install -y nginx
    else
        echo "nginx已安装"
    fi
    
    # 检查并安装基础工具
    for pkg in curl wget unzip ca-certificates gnupg; do
        if ! dpkg -l | grep -q $pkg; then
            echo "安装$pkg..."
            apt-get install -y $pkg
        else
            echo "$pkg已安装"
        fi
    done
    
    # 检查.NET运行时 - 改进检测逻辑
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version 2>/dev/null | head -n 1)
        if [[ "$DOTNET_VERSION" =~ ^9\. ]]; then
            echo ".NET 9.0运行时已安装，版本: $DOTNET_VERSION"
            return 0
        else
            echo "检测到.NET运行时，但版本不是9.0: $DOTNET_VERSION"
            echo "将继续安装.NET 9.0..."
        fi
    else
        echo "未检测到.NET运行时，将安装.NET 9.0..."
    fi
    
    echo "使用二进制方式安装.NET 9.0运行时..."
    
    # 创建安装目录
    mkdir -p /usr/share/dotnet
    cd /tmp
    
    # 使用官方脚本安装.NET 9.0运行时
    echo "尝试使用官方脚本安装.NET 9.0运行时..."
    if curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --channel 9.0 --runtime aspnetcore; then
        # 移动安装的.NET到系统目录
        if [ -d "$HOME/.dotnet" ]; then
            cp -r $HOME/.dotnet/* /usr/share/dotnet/
            ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
            echo "使用官方脚本安装.NET 9.0运行时成功"
            /usr/share/dotnet/dotnet --version
            return 0
        fi
    fi
    
    echo "官方脚本安装失败，尝试直接下载..."
    
    # 直接使用已知的.NET 9.0运行时下载链接
    DOTNET_VERSION="9.0.4"
    DOTNET_PACKAGE="dotnet-runtime-${DOTNET_VERSION}-linux-x64.tar.gz"
    DOTNET_URL="https://download.visualstudio.microsoft.com/download/pr/5f5b2d4a-3c4b-4e2a-8d8a-6b9f8c6d9e1a/8d9a5b6c7d8e9f0a1b2c3d4e5f6a7b8c/${DOTNET_PACKAGE}"
    
    # 尝试下载.NET运行时
    echo "下载.NET 9.0运行时..."
    if ! wget -O ${DOTNET_PACKAGE} ${DOTNET_URL}; then
        echo "使用备用下载链接..."
        # 使用Microsoft官方备用链接
        DOTNET_URL="https://download.microsoft.com/download/6/F/A/6FA49871-8C1C-44E8-9C0A-2C8E4E8C4F5B/${DOTNET_PACKAGE}"
        if ! wget -O ${DOTNET_PACKAGE} ${DOTNET_URL}; then
            echo "使用包管理器安装.NET运行时..."
            # 回退到包管理器安装
            wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
            dpkg -i packages-microsoft-prod.deb
            apt-get update
            apt-get install -y aspnetcore-runtime-9.0
            rm -f packages-microsoft-prod.deb
            return 0
        fi
    fi
    
    # 解压并安装
    echo "解压.NET运行时..."
    tar zxf ${DOTNET_PACKAGE} -C /usr/share/dotnet
    
    # 创建符号链接
    ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
    
    # 添加到系统PATH
    echo 'export PATH=$PATH:/usr/share/dotnet' >> /etc/profile
    echo 'export DOTNET_ROOT=/usr/share/dotnet' >> /etc/profile
    
    # 清理
    rm -f ${DOTNET_PACKAGE}
    
    # 验证安装
    /usr/share/dotnet/dotnet --version
}

# 生成自签名SSL证书
generate_ssl_cert() {
    echo "生成自签名SSL证书..."
    
    # 检查是否已有SSL证书
    if [ -f "/etc/ssl/certs/tts.crt" ] && [ -f "/etc/ssl/private/tts.key" ]; then
        echo "SSL证书已存在，跳过生成。"
        return 0
    fi
    
    # 创建证书目录
    mkdir -p /etc/ssl/certs
    mkdir -p /etc/ssl/private
    
    # 生成私钥
    openssl genrsa -out /etc/ssl/private/tts.key 2048
    
    # 生成证书
    openssl req -new -x509 -key /etc/ssl/private/tts.key -out /etc/ssl/certs/tts.crt -days 365 \
        -subj "/C=CN/ST=State/L=City/O=Organization/CN=localhost"
    
    # 设置权限
    chmod 600 /etc/ssl/private/tts.key
    chmod 644 /etc/ssl/certs/tts.crt
    
    echo "自签名SSL证书已生成。"
    echo "注意：浏览器会警告此证书不受信任，这是正常的。"
}

# 配置Nginx
configure_nginx() {
    echo "配置Nginx..."
    
    # 检查是否需要生成SSL证书
    if [ "$1" = "--https" ]; then
        generate_ssl_cert
    fi
    
    # 检查是否已有SSL证书
    if [ ! -f "/etc/ssl/certs/tts.crt" ] || [ ! -f "/etc/ssl/private/tts.key" ]; then
        echo "未找到SSL证书，使用HTTP配置..."
        cat > $NGINX_CONF << EOF
server {
    listen 80;
    server_name _;
    
    # 安全头
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    # WebApp代理
    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    # API代理
    location /api/ {
        proxy_pass http://127.0.0.1:5002/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    # 静态文件缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF
    else
        echo "找到SSL证书，使用HTTPS配置..."
        cat > $NGINX_CONF << EOF
server {
    listen 80;
    server_name _;
    
    # 重定向到HTTPS
    return 301 https://\$server_name\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name _;
    
    # SSL证书配置
    ssl_certificate /etc/ssl/certs/tts.crt;
    ssl_certificate_key /etc/ssl/private/tts.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    
    # 安全头
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    # WebApp代理
    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    # API代理
    location /api/ {
        proxy_pass http://127.0.0.1:5002/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
    
    # 静态文件缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF
    fi

    # 启用站点
    ln -sf $NGINX_CONF /etc/nginx/sites-enabled/
    rm -f /etc/nginx/sites-enabled/default
    
    # 测试并重启Nginx
    nginx -t && systemctl restart nginx
}

# 配置Supervisor
configure_supervisor() {
    echo "配置Supervisor..."
    
    # 复制配置文件
    cp $DEPLOY_PATH/current/deploy/supervisor/tts-webapp.conf /etc/supervisor/conf.d/
    cp $DEPLOY_PATH/current/deploy/supervisor/tts-webapi.conf /etc/supervisor/conf.d/
    
    # 重新加载配置
    supervisorctl reread
    supervisorctl update
}

# 初始化部署
init_deployment() {
    echo "初始化部署..."
    
    create_directories
    install_dependencies
    
    # 传递HTTPS选项给configure_nginx
    configure_nginx "$1"
    
    echo "初始化完成。请将应用程序文件复制到 $DEPLOY_PATH/current 目录，然后运行 '$0 update' 完成部署。"
}

# 更新部署
update_deployment() {
    echo "更新部署..."
    
    # 备份当前版本
    if [ -d "$DEPLOY_PATH/current" ]; then
        echo "备份当前版本..."
        BACKUP_NAME="backup-$(date +%Y%m%d%H%M%S)"
        cp -r $DEPLOY_PATH/current $BACKUP_DIR/$BACKUP_NAME
    fi
    
    # 配置Supervisor
    configure_supervisor
    
    # 停止服务
    supervisorctl stop tts-webapp tts-webapi || true
    
    # 确保权限正确
    chown -R www-data:www-data $DEPLOY_PATH/current
    chmod +x $DEPLOY_PATH/current/webapp/TtsWebApp
    chmod +x $DEPLOY_PATH/current/webapi/TtsWebApi
    
    # 启动服务
    supervisorctl start tts-webapp tts-webapi
    
    # 检查服务状态
    sleep 5
    supervisorctl status tts-webapp tts-webapi
    
    echo "部署更新完成！"
}

# 回滚部署
rollback_deployment() {
    echo "回滚部署..."
    
    # 获取最新的备份
    LATEST_BACKUP=$(ls -t $BACKUP_DIR | head -n 1)
    
    if [ -z "$LATEST_BACKUP" ]; then
        echo "没有找到可用的备份！"
        exit 1
    fi
    
    echo "回滚到备份: $LATEST_BACKUP"
    
    # 停止服务
    supervisorctl stop tts-webapp tts-webapi || true
    
    # 备份当前版本
    if [ -d "$DEPLOY_PATH/current" ]; then
        mv $DEPLOY_PATH/current $BACKUP_DIR/failed-$(date +%Y%m%d%H%M%S)
    fi
    
    # 恢复备份
    cp -r $BACKUP_DIR/$LATEST_BACKUP $DEPLOY_PATH/current
    
    # 启动服务
    supervisorctl start tts-webapp tts-webapi
    
    # 检查服务状态
    sleep 5
    supervisorctl status tts-webapp tts-webapi
    
    echo "回滚完成！"
}

# 查看状态
show_status() {
    echo "=== 服务状态 ==="
    supervisorctl status tts-webapp tts-webapi
    
    echo -e "\n=== Nginx状态 ==="
    systemctl is-active nginx
    
    echo -e "\n=== .NET版本 ==="
    /usr/share/dotnet/dotnet --version
    
    echo -e "\n=== 最近的日志 ==="
    echo "WebApp日志:"
    tail -n 5 $LOG_DIR/webapp.out.log 2>/dev/null || echo "无日志文件"
    
    echo "WebApi日志:"
    tail -n 5 $LOG_DIR/webapi.out.log 2>/dev/null || echo "无日志文件"
}

# 主函数
case "$1" in
    init)
        init_deployment "$2"
        ;;
    update)
        update_deployment
        ;;
    rollback)
        rollback_deployment
        ;;
    status)
        show_status
        ;;
    *)
        echo "使用方法: $0 {init|update|rollback|status}"
        echo "  init    - 初始化部署环境"
        echo "  init --https - 初始化部署环境并生成自签名SSL证书"
        echo "  update  - 更新部署"
        echo "  rollback- 回滚到上一个版本"
        echo "  status  - 查看服务状态"
        exit 1
        ;;
esac