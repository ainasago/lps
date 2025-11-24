#!/bin/bash

# TTS项目部署脚本 - Supervisor模式
# 使用方法: ./deploy-supervisor.sh [init|update|rollback|status]

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
    echo "安装系统依赖..."
    apt-get update
    apt-get install -y supervisor nginx curl wget unzip
    
    # 检测系统版本并安装.NET运行时
    if [ -f /etc/debian_version ]; then
        echo "检测到Debian系统，使用Debian软件源安装.NET..."
        # 添加Microsoft GPG密钥和Debian软件源
        wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        dpkg -i packages-microsoft-prod.deb
        apt-get update
        apt-get install -y aspnetcore-runtime-9.0
    elif [ -f /etc/lsb-release ]; then
        echo "检测到Ubuntu系统，使用Ubuntu软件源安装.NET..."
        # 添加Microsoft GPG密钥和Ubuntu软件源
        wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        dpkg -i packages-microsoft-prod.deb
        apt-get update
        apt-get install -y aspnetcore-runtime-9.0
    else
        echo "不支持的系统，尝试使用通用安装方法..."
        # 使用snap安装.NET作为备选方案
        apt-get install -y snapd
        snap install dotnet-sdk --classic
        snap install dotnet-runtime-90 --classic
    fi
    
    # 清理
    rm -f packages-microsoft-prod.deb
}

# 配置Nginx
configure_nginx() {
    echo "配置Nginx..."
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
    
    # SSL证书配置 (请替换为您的证书路径)
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
    configure_nginx
    
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
    
    echo -e "\n=== 最近的日志 ==="
    echo "WebApp日志:"
    tail -n 5 $LOG_DIR/webapp.out.log 2>/dev/null || echo "无日志文件"
    
    echo "WebApi日志:"
    tail -n 5 $LOG_DIR/webapi.out.log 2>/dev/null || echo "无日志文件"
}

# 主函数
case "$1" in
    init)
        init_deployment
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
        echo "  update  - 更新部署"
        echo "  rollback- 回滚到上一个版本"
        echo "  status  - 查看服务状态"
        exit 1
        ;;
esac