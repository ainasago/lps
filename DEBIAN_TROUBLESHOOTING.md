# Debian系统部署故障排除指南

本文档专门解决在Debian系统（特别是Debian Trixie测试版）上部署TTS项目时可能遇到的常见问题。

## 常见问题及解决方案

### 1. .NET 9.0运行时安装失败

**问题描述：**
```
The following packages have unmet dependencies:
 dotnet-runtime-deps-9.0 : Depends: libicu but it is not installable or
                           libicu74 but it is not installable or
                           ...
E: Unable to correct problems, you have held broken packages.
```

或者：

```
--2025-11-24 05:18:34--  `https://download.visualstudio.microsoft.com/download/pr/` 
 Connecting to download.visualstudio.microsoft.com... connected
 HTTP request sent, awaiting response... 400 Bad Request
 ERROR 400: Bad Request.
```

**原因分析：**
1. Debian Trixie是测试版，软件包可能不完整或存在依赖冲突
2. .NET运行时下载链接可能已过期或无效
3. libicu库版本可能不匹配

**解决方案：**

#### 方案1：使用专门的Debian部署脚本
```bash
# 使用我们提供的Debian专用脚本
chmod +x deploy/supervisor/deploy-debian.sh
sudo ./deploy/supervisor/deploy-debian.sh init
```

#### 方案2：手动安装.NET运行时
```bash
# 创建安装目录
sudo mkdir -p /usr/share/dotnet
cd /tmp

# 下载.NET 9.0运行时二进制包
wget https://download.visualstudio.microsoft.com/download/pr/5f5b2d4a-3c4b-4e2a-8d8a-6b9f8c6d9e1a/8d9a5b6c7d8e9f0a1b2c3d4e5f6a7b8c/dotnet-runtime-9.0.4-linux-x64.tar.gz

# 如果上述链接失效，尝试使用官方脚本安装
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore

# 解压并安装
sudo tar zxf dotnet-runtime-9.0.4-linux-x64.tar.gz -C /usr/share/dotnet

# 创建符号链接
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# 添加到系统PATH
echo 'export PATH=$PATH:/usr/share/dotnet' | sudo tee -a /etc/profile
echo 'export DOTNET_ROOT=/usr/share/dotnet' | sudo tee -a /etc/profile

# 验证安装
/usr/share/dotnet/dotnet --version
```

#### 方案3：使用官方安装脚本
```bash
# 下载并运行Microsoft官方安装脚本
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore

# 创建符号链接
sudo ln -sf ~/.dotnet/dotnet /usr/bin/dotnet

# 验证安装
dotnet --version
```

#### 方案4：使用Snap安装
```bash
# 安装snap
sudo apt update
sudo apt install -y snapd

# 安装.NET SDK和运行时
sudo snap install dotnet-sdk --classic
sudo snap install dotnet-runtime-90 --classic

# 创建符号链接
sudo ln -sf /snap/bin/dotnet /usr/bin/dotnet
```

#### 方案5：使用包管理器（适用于Debian 12及以上版本）
```bash
# 添加Microsoft软件源
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# 安装.NET运行时
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0

# 清理
rm -f packages-microsoft-prod.deb
```

### .NET运行时检测问题

**问题描述：**
脚本无法正确检测已安装的.NET 9.0运行时，导致重复安装。

**解决方案：**
```bash
# 检查.NET运行时是否正确安装
dotnet --version

# 如果命令不存在，尝试完整路径
/usr/share/dotnet/dotnet --version

# 重新加载PATH
source /etc/profile

# 再次检查
dotnet --version
```

### 2. Docker安装问题

**问题描述：**
Docker在Debian Trixie上可能无法正常安装或启动。

**解决方案：**

#### 方案1：使用Docker官方安装脚本
```bash
# 下载并运行Docker官方安装脚本
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 启动Docker服务
sudo systemctl start docker
sudo systemctl enable docker

# 添加当前用户到docker组
sudo usermod -aG docker $USER
# 重新登录以使组权限生效
```

#### 方案2：使用我们提供的Debian脚本
```bash
# 使用我们提供的Debian专用脚本
chmod +x deploy/docker/deploy-docker.sh
sudo ./deploy/docker/deploy-docker.sh init
```

### 3. Nginx配置问题

**问题描述：**
Nginx可能无法启动或配置不生效。

**解决方案：**
```bash
# 检查Nginx配置语法
sudo nginx -t

# 如果有错误，查看详细错误信息
sudo nginx -T

# 检查端口占用情况
sudo netstat -tlnp | grep :80
sudo netstat -tlnp | grep :443

# 如果端口被占用，停止占用服务或更改Nginx端口
sudo systemctl stop apache2  # 如果Apache占用80端口
# 或编辑Nginx配置文件，更改监听端口
sudo nano /etc/nginx/sites-available/tts
```

#### SSL证书不存在错误

**问题描述：**
Nginx配置测试失败，提示SSL证书文件不存在：
```
nginx: configuration file /etc/nginx/nginx.conf test failed
cannot load certificate "/etc/ssl/certs/tts.crt": BIO_new_file() failed (SSL: error:80000002:system library::No such file or directory:calling fopen(/etc/ssl/certs/tts.crt, r) error:10000080:BIO routines::no such file)
```

**解决方案：**

##### 方案1：使用HTTP模式（无需SSL证书）
```bash
# 使用修复后的脚本初始化HTTP模式
sudo ./deploy/supervisor/deploy-debian.sh init
```

##### 方案2：生成自签名SSL证书
```bash
# 创建证书目录
sudo mkdir -p /etc/ssl/certs
sudo mkdir -p /etc/ssl/private

# 生成私钥
sudo openssl genrsa -out /etc/ssl/private/tts.key 2048

# 生成证书
sudo openssl req -new -x509 -key /etc/ssl/private/tts.key -out /etc/ssl/certs/tts.crt -days 365 \
    -subj "/C=CN/ST=State/L=City/O=Organization/CN=localhost"

# 设置权限
sudo chmod 600 /etc/ssl/private/tts.key
sudo chmod 644 /etc/ssl/certs/tts.crt

# 测试Nginx配置
sudo nginx -t

# 重启Nginx
sudo systemctl restart nginx
```

##### 方案3：使用修复后的脚本初始化HTTPS模式
```bash
# 自动生成证书并配置HTTPS
sudo ./deploy/supervisor/deploy-debian.sh init --https
```

### 4. Supervisor服务无法启动

**问题描述：**
Supervisor管理的服务无法启动或状态异常。

**解决方案：**
```bash
# 检查Supervisor状态
sudo supervisorctl status

# 查看详细错误日志
sudo supervisorctl tail tts-webapp
sudo supervisorctl tail tts-webapi

# 检查配置文件语法
sudo supervisorctl reread
sudo supervisorctl update

# 手动启动服务测试
sudo -u www-data /usr/share/dotnet/dotnet /var/www/tts/current/webapp/TtsWebApp.dll
sudo -u www-data /usr/share/dotnet/dotnet /var/www/tts/current/webapi/TtsWebApi.dll

# 检查文件权限
ls -la /var/www/tts/current/webapp/
ls -la /var/www/tts/current/webapi/

# 确保可执行文件有执行权限
sudo chmod +x /var/www/tts/current/webapp/TtsWebApp
sudo chmod +x /var/www/tts/current/webapi/TtsWebApi
```

### 5. 权限问题

**问题描述：**
应用无法访问某些目录或文件。

**解决方案：**
```bash
# 检查目录权限
ls -la /var/www/tts/
ls -la /var/log/tts/

# 修复权限
sudo chown -R www-data:www-data /var/www/tts/
sudo chown -R www-data:www-data /var/log/tts/

# 确保日志目录存在且有写权限
sudo mkdir -p /var/log/tts
sudo chown www-data:www-data /var/log/tts
sudo chmod 755 /var/log/tts
```

### 6. SSL证书问题

**问题描述：**
HTTPS无法正常工作或证书错误。

**解决方案：**
```bash
# 检查证书文件是否存在
ls -la /etc/ssl/certs/tts.crt
ls -la /etc/ssl/private/tts.key

# 如果证书不存在，生成自签名证书
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/ssl/private/tts.key \
  -out /etc/ssl/certs/tts.crt

# 检查Nginx SSL配置
sudo nginx -t

# 重启Nginx
sudo systemctl restart nginx
```

## 系统兼容性说明

### 支持的系统版本
- Ubuntu 18.04 LTS及以上版本
- Debian 11 (Bullseye)及以上版本
- Debian 12 (Bookworm)及以上版本
- Debian Trixie (测试版，可能需要特殊处理)

### 不推荐使用的系统版本
- Ubuntu 16.04 LTS及以下版本
- Debian 10 (Buster)及以下版本

## 获取帮助

如果以上解决方案无法解决您的问题，请：

1. 收集详细的错误日志
2. 记录系统版本信息
3. 记录执行的具体命令和输出
4. 在项目的GitHub仓库中提交Issue

## 系统信息收集脚本

以下脚本可以帮助收集系统信息，便于问题诊断：

```bash
#!/bin/bash
echo "=== 系统信息 ==="
uname -a
cat /etc/os-release

echo -e "\n=== .NET信息 ==="
which dotnet
dotnet --version

echo -e "\n=== Docker信息 ==="
which docker
docker --version
docker-compose --version

echo -e "\n=== Nginx信息 ==="
which nginx
nginx -v
systemctl status nginx

echo -e "\n=== Supervisor信息 ==="
which supervisorctl
supervisorctl version
systemctl status supervisor

echo -e "\n=== 网络信息 ==="
netstat -tlnp | grep -E ':(80|443|5000|5002)'

echo -e "\n=== 磁盘空间 ==="
df -h
```