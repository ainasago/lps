# SSH密钥配置指南

## 问题概述

在GitHub Actions CI/CD中遇到SSH认证失败错误：
```
ssh: handshake failed: ssh: unable to authenticate, attempted methods [none publickey], no supported methods remain
```

## 解决方案

### 1. 生成正确的SSH密钥

在本地机器上生成新的SSH密钥对（推荐使用ED25519算法）：

```bash
# 生成ED25519密钥对（推荐）
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/github_actions_key

# 或者使用RSA密钥（如果服务器不支持ED25519）
ssh-keygen -t rsa -b 4096 -C "github-actions-deploy" -f ~/.ssh/github_actions_key
```

### 2. 配置服务器端

将公钥添加到服务器的授权密钥中：

```bash
# 将公钥内容复制到服务器
cat ~/.ssh/github_actions_key.pub

# 在服务器上，将公钥添加到授权密钥
mkdir -p ~/.ssh
echo "PASTE_PUBLIC_KEY_HERE" >> ~/.ssh/authorized_keys

# 设置正确的权限
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys
```

### 3. 配置GitHub Secrets

在GitHub仓库中设置以下Secrets：

1. `SERVER_HOST`: 服务器IP地址或域名
2. `SERVER_USERNAME`: SSH登录用户名
3. `SERVER_SSH_KEY`: 私钥内容（包括`-----BEGIN...`和`-----END...`行）
4. `SERVER_PORT`: SSH端口（默认为22）
5. `DEPLOY_PATH`: 部署目录路径

#### 设置私钥Secret的步骤：

1. 查看私钥内容：
   ```bash
   cat ~/.ssh/github_actions_key
   ```

2. 复制整个输出（包括所有行）

3. 在GitHub仓库中：
   - 转到 Settings > Secrets and variables > Actions
   - 点击 "New repository secret"
   - 名称设置为 `SERVER_SSH_KEY`
   - 将复制的私钥内容粘贴到 "Secret" 字段中

### 4. 服务器SSH配置检查

确保服务器SSH配置允许公钥认证：

```bash
# 检查SSH配置
sudo nano /etc/ssh/sshd_config

# 确保以下配置存在且未被注释：
PubkeyAuthentication yes
AuthorizedKeysFile .ssh/authorized_keys

# 重启SSH服务
sudo systemctl restart sshd
```

### 5. 测试SSH连接

在本地测试SSH连接是否正常：

```bash
# 使用生成的密钥测试连接
ssh -i ~/.ssh/github_actions_key username@server_ip

# 如果连接成功，说明密钥配置正确
```

### 6. 常见问题排查

#### 问题1：权限错误
确保私钥文件权限正确：
```bash
chmod 600 ~/.ssh/github_actions_key
```

#### 问题2：服务器不支持ED25519
如果服务器不支持ED25519，使用RSA密钥：
```bash
ssh-keygen -t rsa -b 4096 -C "github-actions-deploy" -f ~/.ssh/github_actions_key
```

#### 问题3：SSH密钥格式问题
确保私钥格式正确，包含完整的BEGIN和END行：
```
-----BEGIN OPENSSH PRIVATE KEY-----
...
-----END OPENSSH PRIVATE KEY-----
```

或
```
-----BEGIN RSA PRIVATE KEY-----
...
-----END RSA PRIVATE KEY-----
```

#### 问题4：服务器防火墙
确保服务器防火墙允许SSH连接：
```bash
# 检查防火墙状态
sudo ufw status

# 如果需要，允许SSH端口
sudo ufw allow 22
```

### 7. 替代方案：使用密码认证

如果无法使用SSH密钥，可以使用密码认证（不推荐，安全性较低）：

1. 在GitHub Secrets中添加 `SERVER_PASSWORD`
2. 修改CI/CD配置，使用密码替代密钥：

```yaml
- name: Deploy to server via SSH
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ${{ secrets.SERVER_USERNAME }}
    password: ${{ secrets.SERVER_PASSWORD }}
    port: ${{ secrets.SERVER_PORT || 22 }}
    script: |
      # 部署脚本
```

### 8. 高级配置

#### 使用SSH代理转发

如果需要通过跳板机连接，可以使用SSH代理转发：

```yaml
- name: Deploy to server via SSH
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.TARGET_SERVER_HOST }}
    username: ${{ secrets.TARGET_SERVER_USERNAME }}
    key: ${{ secrets.TARGET_SERVER_SSH_KEY }}
    proxy_host: ${{ secrets.PROXY_SERVER_HOST }}
    proxy_username: ${{ secrets.PROXY_SERVER_USERNAME }}
    proxy_key: ${{ secrets.PROXY_SERVER_SSH_KEY }}
    proxy_port: ${{ secrets.PROXY_SERVER_PORT || 22 }}
    script: |
      # 部署脚本
```

#### 使用SSH配置文件

如果需要更复杂的SSH配置，可以在脚本中创建SSH配置文件：

```yaml
- name: Deploy to server via SSH
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ${{ secrets.SERVER_USERNAME }}
    key: ${{ secrets.SERVER_SSH_KEY }}
    port: ${{ secrets.SERVER_PORT || 22 }}
    script: |
      # 创建SSH配置
      cat > ~/.ssh/config << EOF
      Host *
          StrictHostKeyChecking no
          UserKnownHostsFile=/dev/null
      EOF
      
      # 部署脚本
```

## 总结

1. 生成新的SSH密钥对（推荐ED25519）
2. 将公钥添加到服务器的authorized_keys文件
3. 将私钥添加为GitHub Secret
4. 确保服务器SSH配置允许公钥认证
5. 测试SSH连接
6. 检查CI/CD日志中的详细错误信息

按照以上步骤，应该能够解决SSH认证失败的问题。如果问题仍然存在，请检查CI/CD日志中的详细错误信息，以便进一步排查。