# GitHub Actions SSH连接故障排除

## 问题诊断

当GitHub Actions中的SSH连接失败时，错误信息可能不够详细。我们已经更新了CI/CD配置，添加了调试模式和超时设置，这将提供更详细的错误信息。

## 常见SSH连接错误及解决方案

### 1. "ssh: unable to authenticate, attempted methods [none publickey], no supported methods remain"

**原因**：SSH密钥认证失败，服务器不接受提供的公钥。

**解决方案**：
1. 检查SSH密钥格式是否正确
2. 确保公钥已正确添加到服务器的authorized_keys文件
3. 验证服务器SSH配置允许公钥认证

### 2. "ssh: connect to host host port port: Connection timed out"

**原因**：网络连接问题或防火墙阻止连接。

**解决方案**：
1. 检查服务器IP地址和端口是否正确
2. 确保服务器防火墙允许SSH连接
3. 检查网络连接是否正常

### 3. "ssh: connect to host host port port: Connection refused"

**原因**：服务器未运行SSH服务或端口不正确。

**解决方案**：
1. 确认SSH服务在服务器上运行：`sudo systemctl status sshd`
2. 检查SSH端口是否正确（默认为22）
3. 确认服务器IP地址是否正确

### 4. "Permission denied (publickey)"

**原因**：SSH密钥权限问题或密钥不匹配。

**解决方案**：
1. 确保私钥文件权限为600：`chmod 600 ~/.ssh/private_key`
2. 确保authorized_keys文件权限为600：`chmod 600 ~/.ssh/authorized_keys`
3. 确保.ssh目录权限为700：`chmod 700 ~/.ssh`

## 调试步骤

### 1. 启用详细日志

我们已在CI/CD配置中添加了`debug: true`，这将提供更详细的SSH连接日志。

### 2. 本地测试SSH连接

在本地测试SSH连接，排除密钥问题：

```bash
# 使用与GitHub Actions相同的密钥测试连接
ssh -i /path/to/private_key username@server_ip -p port -v

# -v 参数提供详细日志，-vv 更详细，-vvv 最详细
```

### 3. 检查服务器SSH配置

登录服务器，检查SSH配置：

```bash
# 查看SSH配置
sudo nano /etc/ssh/sshd_config

# 确保以下配置正确：
PubkeyAuthentication yes
AuthorizedKeysFile .ssh/authorized_keys
PasswordAuthentication no  # 可选，提高安全性

# 重启SSH服务
sudo systemctl restart sshd
```

### 4. 检查服务器日志

查看服务器SSH日志，了解连接失败原因：

```bash
# 查看SSH日志
sudo journalctl -u sshd -f

# 或者查看传统日志文件
sudo tail -f /var/log/auth.log
```

## 高级故障排除

### 1. 使用不同的SSH密钥类型

如果ED25519密钥不工作，尝试RSA密钥：

```bash
# 生成RSA密钥
ssh-keygen -t rsa -b 4096 -C "github-actions-deploy"

# 将公钥添加到服务器
ssh-copy-id -i ~/.ssh/id_rsa.pub username@server_ip
```

### 2. 检查SSH算法兼容性

某些服务器可能不支持最新的SSH算法。在本地测试时指定算法：

```bash
# 测试不同的密钥交换算法
ssh -i ~/.ssh/private_key username@server_ip -o KexAlgorithms=diffie-hellman-group14-sha1

# 测试不同的加密算法
ssh -i ~/.ssh/private_key username@server_ip -o Ciphers=aes128-ctr
```

### 3. 使用SSH配置文件

创建SSH配置文件解决兼容性问题：

```yaml
- name: Deploy to server via SSH
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ${{ secrets.SERVER_USERNAME }}
    key: ${{ secrets.SERVER_SSH_KEY }}
    port: ${{ secrets.SERVER_PORT || 22 }}
    script: |
      # 创建SSH配置文件
      cat > ~/.ssh/config << EOF
      Host *
          StrictHostKeyChecking no
          UserKnownHostsFile=/dev/null
          PubkeyAcceptedKeyTypes=+ssh-rsa
          HostkeyAlgorithms=+ssh-rsa
      EOF
      
      # 设置权限
      chmod 600 ~/.ssh/config
      
      # 测试连接
      ssh -v ${{ secrets.SERVER_USERNAME }}@${{ secrets.SERVER_HOST }} -p ${{ secrets.SERVER_PORT || 22 }} "echo 'SSH connection successful'"
```

## 替代方案

### 1. 使用自托管Runner

如果SSH连接问题持续存在，可以考虑使用自托管的GitHub Actions Runner：

1. 在服务器上安装GitHub Actions Runner
2. 将Runner添加到仓库
3. 修改CI/CD配置使用自托管Runner

```yaml
jobs:
  deploy:
    runs-on: self-hosted  # 使用自托管Runner
    steps:
      - name: Deploy
        run: |
          # 直接在服务器上执行部署命令，无需SSH
```

### 2. 使用部署脚本

创建一个部署脚本，通过HTTP触发器执行：

1. 在服务器上创建一个Web服务，监听部署请求
2. 在GitHub Actions中发送HTTP请求触发部署
3. 服务器端执行部署脚本

```yaml
- name: Trigger deployment
  run: |
    curl -X POST \
      -H "Authorization: Bearer ${{ secrets.DEPLOY_TOKEN }}" \
      -H "Content-Type: application/json" \
      -d '{"ref": "${{ github.ref }}", "commit": "${{ github.sha }}"}' \
      https://your-server.com/deploy
```

## 预防措施

### 1. 定期更新SSH密钥

定期轮换SSH密钥，提高安全性：

```bash
# 生成新密钥
ssh-keygen -t ed25519 -C "github-actions-deploy-$(date +%Y%m%d)"

# 更新服务器上的authorized_keys
# 更新GitHub Secrets中的私钥
```

### 2. 使用短期有效的部署令牌

考虑使用短期有效的部署令牌替代长期有效的SSH密钥：

1. 创建一个令牌生成服务
2. 在CI/CD流程中获取短期令牌
3. 使用令牌进行认证

### 3. 监控SSH连接

设置监控，及时发现SSH连接问题：

```yaml
- name: Check SSH connection
  uses: appleboy/ssh-action@v1.0.3
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ${{ secrets.SERVER_USERNAME }}
    key: ${{ secrets.SERVER_SSH_KEY }}
    port: ${{ secrets.SERVER_PORT || 22 }}
    script: |
      # 检查SSH连接
      echo "SSH connection test at $(date)"
      
      # 检查磁盘空间
      df -h
      
      # 检查内存使用
      free -m
      
      # 检查服务状态
      systemctl status nginx || echo "Nginx not running"
      systemctl status supervisor || echo "Supervisor not running"
```

## 总结

SSH连接问题是GitHub Actions CI/CD中的常见问题，但通过系统性的故障排除，大多数问题都可以解决。关键是要：

1. 仔细检查错误日志
2. 本地测试SSH连接
3. 验证服务器SSH配置
4. 确保密钥格式和权限正确
5. 考虑使用替代方案（如自托管Runner）

如果问题仍然存在，请提供详细的错误日志，以便进一步诊断。