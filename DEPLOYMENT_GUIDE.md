# TTS项目CI/CD自动化部署指南

本指南详细说明了如何使用GitHub Actions实现TTS项目的自动化构建和部署，支持Supervisor和Docker两种部署模式。

## 目录

1. [CI/CD流程概述](#cicd流程概述)
2. [GitHub Secrets配置](#github-secrets配置)
3. [Supervisor部署模式](#supervisor部署模式)
4. [Docker部署模式](#docker部署模式)
5. [故障排除](#故障排除)

## CI/CD流程概述

我们的CI/CD流程包含以下步骤：

1. **代码检查和构建**：当代码推送到main或develop分支时，自动触发构建
2. **测试**：运行单元测试确保代码质量
3. **Docker镜像构建**：构建并推送Docker镜像到GitHub Container Registry
4. **自动部署**：根据配置自动部署到服务器（支持Supervisor和Docker两种模式）

### 工作流触发条件

- 推送到`main`或`develop`分支：触发完整CI/CD流程
- 创建针对`main`分支的Pull Request：仅触发构建和测试

## GitHub Secrets配置

在GitHub仓库的Settings > Secrets and variables > Actions中配置以下密钥：

### Supervisor部署模式所需密钥

| 密钥名称 | 描述 | 示例值 |
|---------|------|--------|
| `SERVER_HOST` | 服务器IP地址或域名 | `192.168.1.100` |
| `SERVER_USERNAME` | 服务器登录用户名 | `ubuntu` |
| `SERVER_SSH_KEY` | SSH私钥内容 | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `SERVER_PORT` | SSH端口（可选，默认22） | `22` |
| `DEPLOY_PATH` | 服务器上的部署路径 | `/var/www/tts` |

### Docker部署模式所需密钥

| 密钥名称 | 描述 | 示例值 |
|---------|------|--------|
| `DOCKER_SERVER_HOST` | Docker服务器IP地址或域名 | `192.168.1.101` |
| `DOCKER_SERVER_USERNAME` | Docker服务器登录用户名 | `ubuntu` |
| `DOCKER_SERVER_SSH_KEY` | SSH私钥内容 | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `DOCKER_SERVER_PORT` | SSH端口（可选，默认22） | `22` |
| `DOCKER_DEPLOY_PATH` | Docker部署路径 | `/opt/tts` |

### SSH密钥生成

```bash
# 在本地生成SSH密钥对
ssh-keygen -t rsa -b 4096 -C "github-actions-deploy"

# 将公钥添加到服务器的authorized_keys
cat ~/.ssh/id_rsa.pub >> ~/.ssh/authorized_keys

# 将私钥内容复制到GitHub Secrets
cat ~/.ssh/id_rsa
```

## Supervisor部署模式

Supervisor是一个进程控制系统，用于管理和监控应用程序进程。

### 服务器环境要求

- Ubuntu 20.04或更高版本
- .NET 9.0运行时
- Nginx
- Supervisor

### 初始化部署

1. 连接到服务器：
```bash
ssh username@your-server-ip
```

2. 下载部署脚本：
```bash
# 如果您有Git访问权限
git clone https://github.com/your-username/tts_turi.git /tmp/tts_turi
cp -r /tmp/tts_turi/deploy/supervisor /opt/
rm -rf /tmp/tts_turi

# 或者手动创建目录和上传脚本
mkdir -p /opt/supervisor
# 上传deploy-supervisor.sh和配置文件
```

3. 运行初始化脚本：
```bash
chmod +x /opt/supervisor/deploy-supervisor.sh
sudo /opt/supervisor/deploy-supervisor.sh init
```

### 手动部署步骤

如果需要手动部署，可以按照以下步骤：

1. 创建应用目录：
```bash
sudo mkdir -p /var/www/tts
sudo chown www-data:www-data /var/www/tts
```

2. 安装.NET 9.0运行时：
```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
rm packages-microsoft-prod.deb
```

3. 安装和配置Nginx：
```bash
sudo apt-get install -y nginx
# 复制nginx配置文件
sudo cp /var/www/tts/current/deploy/nginx/tts.conf /etc/nginx/sites-available/
sudo ln -s /etc/nginx/sites-available/tts.conf /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl restart nginx
```

4. 安装和配置Supervisor：
```bash
sudo apt-get install -y supervisor
# 复制supervisor配置文件
sudo cp /var/www/tts/current/deploy/supervisor/tts-*.conf /etc/supervisor/conf.d/
sudo supervisorctl reread
sudo supervisorctl update
sudo supervisorctl start tts-webapp tts-webapi
```

### 管理命令

```bash
# 查看服务状态
sudo /opt/supervisor/deploy-supervisor.sh status

# 更新部署
sudo /opt/supervisor/deploy-supervisor.sh update

# 回滚到上一个版本
sudo /opt/supervisor/deploy-supervisor.sh rollback
```

## Docker部署模式

Docker模式使用容器化部署，提供更好的隔离性和可移植性。

### 服务器环境要求

- Ubuntu 20.04或更高版本
- Docker Engine 20.10+
- Docker Compose v2.0+

### 初始化部署

1. 连接到服务器：
```bash
ssh username@your-server-ip
```

2. 下载部署脚本：
```bash
# 如果您有Git访问权限
git clone https://github.com/your-username/tts_turi.git /tmp/tts_turi
cp -r /tmp/tts_turi/deploy/docker /opt/
cp /tmp/tts_turi/docker-compose.yml /opt/
cp /tmp/tts_turi/nginx/nginx.conf /opt/nginx/
rm -rf /tmp/tts_turi

# 或者手动创建目录和上传脚本
mkdir -p /opt/docker
# 上传deploy-docker.sh、docker-compose.yml和nginx配置文件
```

3. 运行初始化脚本：
```bash
chmod +x /opt/docker/deploy-docker.sh
sudo /opt/docker/deploy-docker.sh init
```

4. 修改docker-compose.yml中的镜像仓库地址：
```bash
# 将REPO_OWNER替换为您的GitHub用户名或组织名
sed -i 's/REPO_OWNER/your-github-username/g' /opt/docker-compose.yml
```

### 手动部署步骤

如果需要手动部署，可以按照以下步骤：

1. 安装Docker和Docker Compose：
```bash
# 安装Docker
sudo apt-get update
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io

# 安装Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/download/v2.12.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 启动Docker服务
sudo systemctl enable docker
sudo systemctl start docker
```

2. 创建必要的目录：
```bash
sudo mkdir -p /opt/tts/nginx/ssl
sudo mkdir -p /opt/tts/data/webapp
sudo mkdir -p /opt/tts/data/webapi
sudo mkdir -p /opt/tts/logs/webapp
sudo mkdir -p /opt/tts/logs/webapi
```

3. 生成SSL证书（自签名）：
```bash
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /opt/tts/nginx/ssl/key.pem \
    -out /opt/tts/nginx/ssl/cert.pem \
    -subj "/C=CN/ST=State/L=City/O=Organization/CN=localhost"
```

4. 登录到GitHub Container Registry：
```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u your-github-username --password-stdin
```

5. 启动服务：
```bash
cd /opt/tts
docker-compose up -d
```

### 管理命令

```bash
# 查看容器状态
sudo /opt/docker/deploy-docker.sh status

# 更新部署
sudo /opt/docker/deploy-docker.sh update

# 回滚到上一个版本
sudo /opt/docker/deploy-docker.sh rollback

# 查看日志
sudo /opt/docker/deploy-docker.sh logs webapp
sudo /opt/docker/deploy-docker.sh logs webapi
sudo /opt/docker/deploy-docker.sh logs nginx
```

## 故障排除

### 常见问题

1. **部署失败：权限不足**
   - 确保SSH密钥正确设置
   - 确保服务器用户有足够的权限执行部署脚本

2. **Supervisor服务无法启动**
   - 检查应用路径是否正确
   - 查看Supervisor日志：`sudo tail -f /var/log/supervisor/supervisord.log`

3. **Docker容器无法启动**
   - 检查Docker日志：`docker-compose logs`
   - 确保端口没有被占用
   - 检查镜像是否正确拉取

4. **Nginx配置错误**
   - 测试Nginx配置：`sudo nginx -t`
   - 查看Nginx错误日志：`sudo tail -f /var/log/nginx/error.log`

### 日志位置

#### Supervisor模式

- WebApp应用日志：`/var/log/tts/webapp.out.log`
- WebApi应用日志：`/var/log/tts/webapi.out.log`
- Supervisor日志：`/var/log/supervisor/supervisord.log`
- Nginx日志：`/var/log/nginx/`

#### Docker模式

- 所有容器日志：`docker-compose logs`
- 特定服务日志：`docker-compose logs [service-name]`

### 性能优化

1. **数据库优化**
   - 考虑使用外部数据库而不是SQLite
   - 配置适当的连接池

2. **静态文件缓存**
   - Nginx已配置静态文件缓存
   - 考虑使用CDN加速静态资源

3. **负载均衡**
   - 对于高流量场景，考虑部署多个实例并使用负载均衡器

## 安全建议

1. **定期更新系统和依赖**
2. **使用强密码和SSH密钥认证**
3. **配置防火墙限制访问**
4. **定期备份数据**
5. **监控应用日志和系统资源使用情况**

## 联系支持

如果在部署过程中遇到问题，请：

1. 检查本文档的故障排除部分
2. 查看GitHub Issues
3. 提交新的Issue并提供详细的错误信息和日志