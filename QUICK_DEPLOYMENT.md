# TTS项目CI/CD快速配置指南

本指南提供快速配置GitHub Actions CI/CD自动化的步骤，支持Supervisor和Docker两种部署模式。

## 快速开始

### 1. 准备GitHub Secrets

在GitHub仓库的Settings > Secrets and variables > Actions中配置以下密钥：

#### 对于Supervisor部署模式
```
SERVER_HOST=你的服务器IP
SERVER_USERNAME=服务器用户名
SERVER_SSH_KEY=你的SSH私钥
SERVER_PORT=22 (可选，默认为22)
DEPLOY_PATH=/var/www/tts (部署路径)
```

#### 对于Docker部署模式
```
DOCKER_SERVER_HOST=你的Docker服务器IP
DOCKER_SERVER_USERNAME=服务器用户名
DOCKER_SERVER_SSH_KEY=你的SSH私钥
DOCKER_SERVER_PORT=22 (可选，默认为22)
DOCKER_DEPLOY_PATH=/opt/tts (Docker部署路径)
```

### 2. 服务器初始化

#### Supervisor模式
```bash
# 1. 连接到服务器
ssh username@your-server-ip

# 2. 下载部署脚本
git clone https://github.com/your-username/tts_turi.git /tmp/tts_turi
sudo mkdir -p /opt/supervisor
sudo cp /tmp/tts_turi/deploy/supervisor/* /opt/supervisor/
sudo chmod +x /opt/supervisor/deploy-supervisor.sh

# 3. 运行初始化
sudo /opt/supervisor/deploy-supervisor.sh init
```

#### Docker模式
```bash
# 1. 连接到服务器
ssh username@your-server-ip

# 2. 下载部署脚本
git clone https://github.com/your-username/tts_turi.git /tmp/tts_turi
sudo mkdir -p /opt/docker
sudo cp /tmp/tts_turi/deploy/docker/* /opt/docker/
sudo cp /tmp/tts_turi/docker-compose.yml /opt/
sudo cp /tmp/tts_turi/nginx/nginx.conf /opt/nginx/
sudo chmod +x /opt/docker/deploy-docker.sh

# 3. 运行初始化
sudo /opt/docker/deploy-docker.sh init

# 4. 修改docker-compose.yml中的REPO_OWNER为你的GitHub用户名
sudo sed -i 's/REPO_OWNER/your-github-username/g' /opt/docker-compose.yml
```

### 3. 推送代码触发部署

将代码推送到main分支即可触发自动部署：
```bash
git add .
git commit -m "Setup CI/CD"
git push origin main
```

## 部署管理命令

### Supervisor模式
```bash
# 查看状态
sudo /opt/supervisor/deploy-supervisor.sh status

# 手动更新
sudo /opt/supervisor/deploy-supervisor.sh update

# 回滚版本
sudo /opt/supervisor/deploy-supervisor.sh rollback
```

### Docker模式
```bash
# 查看状态
sudo /opt/docker/deploy-docker.sh status

# 手动更新
sudo /opt/docker/deploy-docker.sh update

# 回滚版本
sudo /opt/docker/deploy-docker.sh rollback

# 查看日志
sudo /opt/docker/deploy-docker.sh logs webapp
```

## 注意事项

1. 确保服务器防火墙已正确配置（开放80、443端口）
2. 对于生产环境，建议使用有效的SSL证书替换自签名证书
3. 定期备份应用数据和配置文件
4. 监控服务器资源使用情况

## 故障排除

如果部署失败，请检查：
1. GitHub Secrets是否正确配置
2. 服务器SSH连接是否正常
3. 部署脚本是否有执行权限
4. 应用日志是否有错误信息

详细文档请参考 [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md)