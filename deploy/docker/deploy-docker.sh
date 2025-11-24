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

# 安装Docker和Docker Compose
install_docker() {
    echo "安装Docker和Docker Compose..."
    
    # 安装Docker
    apt-get update
    apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
    
    # 添加Docker官方GPG密钥
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    
    # 添加Docker仓库
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
    
    # 安装Docker Engine
    apt-get update
    apt-get install -y docker-ce docker-ce-cli containerd.io
    
    # 安装Docker Compose
    curl -L "https://github.com/docker/compose/releases/download/v2.12.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    
    # 启动Docker服务
    systemctl enable docker
    systemctl start docker
    
    # 添加当前用户到docker组
    usermod -aG docker $USER
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
    install_docker
    generate_ssl_cert
    
    echo "初始化完成。请将docker-compose.yml文件复制到 $DEPLOY_PATH 目录，然后运行 '$0 update' 完成部署。"
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