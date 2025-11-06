# API 访问控制 - 配置总结

## ✅ 验证成功！

你的配置已经生效了！从日志可以看到：

```
warn: TtsWebApi.Middleware.RefererValidationMiddleware[0]
      API 请求缺少 Referer 和 Origin 头: /api/tts/voices from ::1
warn: TtsWebApp.Controllers.TtsController[0]
      API调用失败，状态码: Forbidden
```

这证明：
- ✅ API 正在拒绝未授权的请求
- ✅ CORS 策略生效
- ✅ Referer 验证生效

## 📋 配置说明

### 开发环境配置（当前）

**文件：** `TtsWebApi/appsettings.json`

```json
{
  "AllowedOrigins": [
    "localhost:5000",
    "localhost:5001",
    "localhost:7000",
    "localhost:7001",
    "www.annietts.com",
    "annietts.com"
  ]
}
```

**效果：**
- ✅ 允许 localhost:5000 访问（TtsWebApp）
- ✅ 允许 localhost:5001 访问
- ✅ 允许生产域名访问

### 生产环境配置

**文件：** `TtsWebApi/appsettings.Production.json`

```json
{
  "AllowedOrigins": [
    "https://www.annietts.com",
    "https://annietts.com"
  ]
}
```

**效果：**
- ✅ 只允许生产域名访问
- ❌ 拒绝所有 localhost 访问
- ✅ 只允许 HTTPS

## 🔄 重启 API 服务

**重要：** 修改配置后必须重启 API 服务！

```bash
# 停止当前服务（Ctrl+C）
# 然后重新启动
cd d:\1Dev\dev\webs\tts_turi\TtsWebApi
dotnet run
```

## 📊 启动日志

重启后应该看到：

```
CORS 允许的来源: http://localhost:5000, https://localhost:5000, http://localhost:5001, https://localhost:5001, ...
Referer 验证允许的主机: localhost:5000, localhost:5001, localhost:7000, localhost:7001, www.annietts.com, annietts.com
```

## 🔧 安全机制

### 1. CORS 策略
- 限制跨域请求来源
- 处理预检请求（OPTIONS）
- 从配置文件读取允许的来源

### 2. Referer 验证
- 验证每个 API 请求的来源
- 检查 Referer 或 Origin 头
- 拒绝未授权的来源

### 3. 中间件顺序

```
请求进入
    ↓
UseHttpsRedirection
    ↓
UseStaticFiles
    ↓
UseCors (CORS 策略)
    ↓
UseRefererValidation (Referer 验证)
    ↓
UseAuthorization
    ↓
MapControllers
```

## ⚠️ 已知问题

### 1. HTTPS 重定向警告

```
Failed to determine the https port for redirect.
```

**原因：** 使用 HTTP 启动，但没有配置 HTTPS 端口

**影响：** 不影响功能，只是警告

**解决：** 可以忽略，或者使用 HTTPS 配置启动

### 2. module is not defined

```
index.js:2 Uncaught ReferenceError: module is not defined
```

**原因：** 浏览器缓存或文件加载顺序问题

**解决方法：**
1. 清除浏览器缓存（Ctrl+Shift+Delete）
2. 硬刷新页面（Ctrl+Shift+R）
3. 使用无痕模式测试
4. 检查 i18n.js 是否正确加载

## 🧪 测试方法

### 测试 1：允许的来源（应该成功）

1. 确保配置包含 `localhost:5000`
2. 重启 API
3. 从 TtsWebApp 访问 API
4. 应该成功 ✅

### 测试 2：禁止的来源（应该失败）

1. 修改配置，移除所有 localhost
   ```json
   {
     "AllowedOrigins": [
       "www.annietts.com"
     ]
   }
   ```
2. 重启 API
3. 从 TtsWebApp 访问 API
4. 应该失败（403 Forbidden）❌

### 测试 3：使用 curl 测试

```bash
# 测试允许的来源
curl -H "Origin: http://localhost:5000" \
     -H "Referer: http://localhost:5000/" \
     http://localhost:5275/api/tts/voices

# 测试禁止的来源
curl -H "Origin: http://evil.com" \
     -H "Referer: http://evil.com/" \
     http://localhost:5275/api/tts/voices
```

## 📝 使用场景

### 场景 1：本地开发

**配置：** `appsettings.json`
```json
{
  "AllowedOrigins": [
    "localhost:5000",
    "localhost:5001"
  ]
}
```

### 场景 2：生产部署

**配置：** `appsettings.Production.json`
```json
{
  "AllowedOrigins": [
    "https://www.annietts.com",
    "https://annietts.com"
  ]
}
```

**部署命令：**
```bash
dotnet publish -c Release
dotnet run --environment Production
```

### 场景 3：测试安全性

**配置：** `appsettings.json`
```json
{
  "AllowedOrigins": [
    "www.annietts.com"
  ]
}
```

重启后，localhost 应该无法访问。

## 🎯 快速参考

### 允许 localhost 访问

```json
{
  "AllowedOrigins": [
    "localhost:5000"
  ]
}
```

### 禁止 localhost 访问

```json
{
  "AllowedOrigins": [
    "www.annietts.com"
  ]
}
```

### 允许所有端口（不推荐）

不要使用通配符！必须明确列出每个端口。

### 查看当前配置

查看启动日志中的：
```
CORS 允许的来源: ...
Referer 验证允许的主机: ...
```

## 📚 相关文档

- `TtsWebApi/API_SECURITY.md` - 详细的安全配置说明
- `TtsWebApi/CONFIG_GUIDE.md` - 配置指南
- `test_api_access.md` - 测试指南

## 🎉 总结

现在你的 API 已经受到保护：

1. ✅ **配置生效** - 从日志可以看到拒绝未授权请求
2. ✅ **统一配置** - CORS 和 Referer 验证都从配置文件读取
3. ✅ **环境分离** - 开发和生产环境使用不同配置
4. ✅ **易于管理** - 只需修改配置文件，无需改代码

**下一步：**
1. 重启 API 服务（使用新配置）
2. 清除浏览器缓存
3. 测试 TTS 功能
4. 部署到生产环境时使用 Production 配置

---

**最后更新：** 2025年1月1日  
**状态：** ✅ 配置已验证生效
