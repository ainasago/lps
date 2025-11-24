# Debian系统快速部署指南

## 概述

本指南针对Debian Trixie系统提供了TTS项目的快速部署方法，解决了.NET 9.0运行时安装和SSL证书配置问题。

## 修复内容

1. **改进的.NET运行时检测**：现在可以正确识别已安装的.NET 9.0运行时
2. **自动SSL证书生成**：可选择生成自签名SSL证书，避免证书不存在导致的Nginx配置错误
3. **多种.NET安装方式**：优先使用官方脚本安装，失败后回退到直接下载和包管理器安装
4. **智能依赖检测**：避免重复安装已存在的依赖项

## 部署步骤

### 1. 服务器初始化

#### Supervisor模式部署

```bash
# 下载并运行部署脚本
chmod +x deploy/supervisor/deploy-debian.sh

# HTTP模式部署（无需SSL证书）
sudo ./deploy/supervisor/deploy-debian.sh init

# HTTPS模式部署（自动生成自签名证书）
sudo ./deploy/supervisor/deploy-debian.sh init --https
```

#### Docker模式部署

```bash
# 下载并运行部署脚本
chmod +x deploy/docker/deploy-docker.sh

# 初始化Docker环境
sudo ./deploy/docker/deploy-docker.sh init
```

### 2. 配置GitHub Secrets

在GitHub仓库设置中添加以下Secrets：

```
DEPLOY_HOST=服务器IP地址
DEPLOY_USER=部署用户名（如root）
DEPLOY_PASSWORD=部署用户密码
DEPLOY_PATH=部署路径（如/var/www/tts）
```

### 3. 触发部署

推送代码到GitHub仓库，自动触发CI/CD流程：

```bash
git add .
git commit -m "触发部署"
git push origin main
```

## 部署管理命令

### Supervisor模式

```bash
# 更新部署
sudo ./deploy/supervisor/deploy-debian.sh update

# 回滚部署
sudo ./deploy/supervisor/deploy-debian.sh rollback

# 查看状态
sudo ./deploy/supervisor/deploy-debian.sh status
```

### Docker模式

```bash
# 更新部署
sudo ./deploy/docker/deploy-docker.sh update

# 回滚部署
sudo ./deploy/docker/deploy-docker.sh rollback

# 查看状态
sudo ./deploy/docker/deploy-docker.sh status
```

## 注意事项

1. **首次部署**：必须先运行`init`命令初始化环境
2. **HTTPS证书**：使用`init --https`会生成自签名证书，浏览器会提示安全警告
3. **.NET检测**：脚本会自动检测已安装的.NET 9.0运行时，避免重复安装
4. **权限问题**：确保使用root权限运行脚本

## 故障排除

### .NET 9.0运行时安装失败

如果.NET运行时安装失败，可以尝试手动安装：

```bash
# 使用官方脚本安装
curl -sSL https://dot.net/v1/dotnet-install.sh | bash

# 或者使用包管理器
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update
apt-get install -y aspnetcore-runtime-9.0
```

### Nginx配置错误

如果Nginx配置测试失败，检查SSL证书是否存在：

```bash
# 检查证书文件
ls -la /etc/ssl/certs/tts.crt
ls -la /etc/ssl/private/tts.key

# 重新生成证书
sudo ./deploy/supervisor/deploy-debian.sh init --https
```

### 服务无法启动

检查服务状态和日志：

```bash
# 查看Supervisor服务状态
sudo supervisorctl status

# 查看应用日志
tail -f /var/log/tts/webapp.out.log
tail -f /var/log/tts/webapi.out.log
```

## 系统兼容性

- **Debian Trixie**：完全支持，推荐使用
- **Debian Bookworm**：支持，可能需要调整软件源
- **Ubuntu 20.04/22.04**：使用标准部署脚本

## 更多信息

- 完整部署指南：[DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
- 故障排除指南：[DEBIAN_TROUBLESHOOTING.md](DEBIAN_TROUBLESHOOTING.md)