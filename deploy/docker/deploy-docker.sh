#!/bin/bash

# TTS项目部署脚本 - Docker模式
# 使用方法: ./deploy-docker.sh [init|update|rollback|status|logs]

set -e

# 配置变量
DEPLOY_PATH="/opt/tts"
COMPOSE_FILE="$DEPLOY_PATH/docker-compose.yml"
NGINX_CONF_DIR="$DEPLOY_PATH/nginx"
SSL_DIR="$DEPLOY_PATH/nginx/ssl"
DATA_DIR="$DEPLOY_PATH/data"
LOG_DIR="$DEPLOY_PATH/logs"

# 确保脚本以root权限运行
if [ "$EUID" -ne 0 ]; then
  echo "请使用root权限运行此脚本"
  exit 1
fi

# 创建必要的目录
create_directories() {
    echo "创建必要的目录..."
    mkdir -p $DEPLOY_PATH
    mkdir -p $NGINX_CONF_DIR
    mkdir -p $SSL_DIR
    mkdir -p $DATA_DIR/webapp
    mkdir -p $DATA_DIR/webapi
    mkdir -p $LOG_DIR/webapp
    mkdir -p $LOG_DIR/webapi
    
    # 设置权限
    chown -R root:root $DEPLOY_PATH
}

# 安装依赖
install_dependencies() {
    echo "检查系统依赖..."
    
    # 检查并安装基础工具
    for pkg in curl wget unzip git; do
        if ! dpkg -l | grep -q $pkg; then
            echo "安装$pkg..."
            apt-get install -y $pkg
        else
            echo "$pkg已安装"
        fi
    done
    
    # 检查Docker
    if command -v docker &> /dev/null; then
        echo "Docker已安装"
        docker --version
    else
        echo "安装Docker..."
        # 检测系统类型
        if [ -f /etc/debian_version ]; then
            echo "检测到Debian系统，使用Debian安装方式..."
            # 安装依赖
            apt-get install -y ca-certificates curl gnupg lsb-release
            
            # 添加Docker官方GPG密钥
            install -m 0755 -d /etc/apt/keyrings
            curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
            chmod a+r /etc/apt/keyrings/docker.gpg
            
            # 添加Docker软件源
            echo \
              "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian \\
              $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \\
              tee /etc/apt/sources.list.d/docker.list > /dev/null
            apt-get update
            apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        elif [ -f /etc/lsb-release ]; then
            echo "检测到Ubuntu系统，使用Ubuntu安装方式..."
            # 安装依赖
            apt-get install -y ca-certificates curl gnupg lsb-release
            
            # 添加Docker官方GPG密钥
            install -m 0755 -d /etc/apt/keyrings
            curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
            chmod a+r /etc/apt/keyrings/docker.gpg
            
            # 添加Docker软件源
            echo \
              "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \\
              $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \\
              tee /etc/apt/sources.list.d/docker.list > /dev/null
            apt-get update
            apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        else
            echo "不支持的系统，尝试使用通用安装脚本..."
            # 使用Docker官方安装脚本
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
        fi
        
        # 启动Docker服务
        systemctl start docker
        systemctl enable docker
        
        # 添加当前用户到docker组
        usermod -aG docker $USER
    fi
    
    # 检查Docker Compose
    if command -v docker-compose &> /dev/null || docker compose version &> /dev/null; then
        echo "Docker Compose已安装"
        if command -v docker-compose &> /dev/null; then
            docker-compose --version
        else
            docker compose version
        fi
    else
        echo "安装Docker Compose..."
        # 对于新版本的Docker，docker compose是内置插件
        # 如果没有，则单独安装
        curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        chmod +x /usr/local/bin/docker-compose
    fi
}

# 生成自签名SSL证书
generate_ssl_cert() {
    echo "生成自签名SSL证书..."
    
    if [ ! -f "$SSL_DIR/cert.pem" ] || [ ! -f "$SSL_DIR/key.pem" ]; then
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout $SSL_DIR/key.pem \
            -out $SSL_DIR/cert.pem \
            -subj "/C=CN/ST=State/L=City/O=Organization/CN=localhost"
    fi
}

# 初始化部署
init_deployment() {
    echo "初始化Docker部署..."
    
    create_directories
    install_dependencies
    generate_ssl_cert
    
    echo "初始化完成。请将应用程序文件复制到 $DEPLOY_PATH 目录，然后运行 '$0 update' 完成部署。"
}

# 更新部署
update_deployment() {
    echo "更新Docker部署..."
    
    cd $DEPLOY_PATH
    
    # 停止现有容器
    docker-compose down || true
    
    # 拉取最新镜像
    docker-compose pull
    
    # 启动服务
    docker-compose up -d
    
    # 等待服务启动
    sleep 10
    
    # 显示容器状态
    docker-compose ps
    
    echo "部署更新完成！"
}

# 回滚部署
rollback_deployment() {
    echo "回滚Docker部署..."
    
    cd $DEPLOY_PATH
    
    # 获取之前运行的镜像标签
    PREVIOUS_IMAGE=$(docker images --format "table {{.Repository}}:{{.Tag}}" | grep tts_turi | tail -n 2 | head -n 1)
    
    if [ -z "$PREVIOUS_IMAGE" ]; then
        echo "没有找到可回滚的镜像！"
        exit 1
    fi
    
    echo "回滚到镜像: $PREVIOUS_IMAGE"
    
    # 停止现有容器
    docker-compose down
    
    # 修改docker-compose.yml使用之前的镜像
    sed -i "s|ghcr.io/.*/tts_turi-.*:latest|$PREVIOUS_IMAGE|g" docker-compose.yml
    
    # 启动服务
    docker-compose up -d
    
    # 等待服务启动
    sleep 10
    
    # 显示容器状态
    docker-compose ps
    
    echo "回滚完成！"
}

# 查看状态
show_status() {
    echo "=== 容器状态 ==="
    cd $DEPLOY_PATH
    docker-compose ps
    
    echo -e "\n=== 资源使用情况 ==="
    docker stats --no-stream
    
    echo -e "\n=== 最近的日志 ==="
    echo "WebApp日志:"
    docker-compose logs --tail=5 tts-webapp
    
    echo "WebApi日志:"
    docker-compose logs --tail=5 tts-webapi
}

# 查看日志
show_logs() {
    cd $DEPLOY_PATH
    
    case "$2" in
        webapp)
            docker-compose logs -f tts-webapp
            ;;
        webapi)
            docker-compose logs -f tts-webapi
            ;;
        nginx)
            docker-compose logs -f tts-nginx
            ;;
        *)
            echo "使用方法: $0 logs {webapp|webapi|nginx}"
            exit 1
            ;;
    esac
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
    logs)
        show_logs "$@"
        ;;
    *)
        echo "使用方法: $0 {init|update|rollback|status|logs}"
        echo "  init    - 初始化Docker部署环境"
        echo "  update  - 更新部署"
        echo "  rollback- 回滚到上一个版本"
        echo "  status  - 查看容器状态"
        echo "  logs    - 查看日志 (webapp|webapi|nginx)"
        exit 1
        ;;
esac