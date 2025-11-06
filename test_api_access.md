# API 访问控制测试指南

## 当前配置

查看 `TtsWebApi/appsettings.json`：
```json
{
  "AllowedOrigins": [
    "www.annietts.com",
    "annietts.com"
  ]
}
```

## 测试步骤

### 1. 重启 API 服务

**重要：** 必须重启服务才能使配置生效！

```bash
# 停止当前运行的 API
Ctrl+C

# 重新启动
cd d:\1Dev\dev\webs\tts_turi\TtsWebApi
dotnet run
```

### 2. 查看启动日志

启动后应该看到类似的日志：

```
CORS 允许的来源: http://www.annietts.com, https://www.annietts.com, http://annietts.com, https://annietts.com
Referer 验证允许的主机: www.annietts.com, annietts.com
```

**如果看到 localhost，说明配置没有生效！**

### 3. 测试从 TtsWebApp 访问

1. 启动 TtsWebApp（端口 5000）
2. 访问 TTS 页面
3. 尝试生成语音

**预期结果：**
- ❌ 应该失败（403 Forbidden 或 CORS 错误）
- 因为 localhost:5000 不在允许列表中

### 4. 查看浏览器控制台

打开浏览器开发者工具（F12），应该看到类似错误：

```
Access to fetch at 'http://localhost:5001/api/tts/convert' from origin 'http://localhost:5000' 
has been blocked by CORS policy: Response to preflight request doesn't pass access control check
```

或者：

```
POST http://localhost:5001/api/tts/convert 403 (Forbidden)
```

### 5. 查看 API 日志

API 控制台应该显示：

```
[Warning] API 请求来自未授权的来源: http://localhost:5000 for /api/tts/convert
```

## 如果还是可以访问

### 检查清单

- [ ] **是否重启了 API 服务？**
  - 必须停止并重新启动，不能热重载

- [ ] **检查启动日志**
  - 查看 "CORS 允许的来源" 是否只包含生产域名
  - 如果包含 localhost，说明配置没有生效

- [ ] **检查是否有多个 API 实例在运行**
  ```bash
  # Windows
  netstat -ano | findstr :5001
  
  # 如果有多个进程，杀掉旧的
  taskkill /PID <进程ID> /F
  ```

- [ ] **检查配置文件是否正确保存**
  - 确认 appsettings.json 没有语法错误
  - 确认文件已保存（Ctrl+S）

- [ ] **清除浏览器缓存**
  - 按 Ctrl+Shift+Delete
  - 清除缓存和 Cookie
  - 或使用无痕模式测试

### 调试步骤

1. **添加断点**
   - 在 `Program.cs` 第 42 行添加断点
   - 查看 `allowedUrls` 的值

2. **查看详细日志**
   - 修改 `appsettings.json`：
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "Microsoft.AspNetCore": "Debug"
       }
     }
   }
   ```

3. **测试 CORS 预检**
   ```bash
   curl -X OPTIONS \
        -H "Origin: http://localhost:5000" \
        -H "Access-Control-Request-Method: POST" \
        -v \
        http://localhost:5001/api/tts/voices
   ```
   
   **预期结果：** 应该没有 `Access-Control-Allow-Origin` 头

4. **测试实际请求**
   ```bash
   curl -X GET \
        -H "Origin: http://localhost:5000" \
        -H "Referer: http://localhost:5000/" \
        -v \
        http://localhost:5001/api/tts/voices
   ```
   
   **预期结果：** 403 Forbidden

## 恢复开发环境配置

如果需要恢复开发环境，修改 `appsettings.json`：

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

然后重启 API 服务。

## 中间件执行顺序

现在的中间件顺序：

```
请求进入
    ↓
UseHttpsRedirection
    ↓
UseStaticFiles
    ↓
UseCors (CORS 策略检查)
    ↓
UseRefererValidation (Referer 验证)
    ↓
UseAuthorization
    ↓
MapControllers
```

**关键点：**
- CORS 在 Referer 验证之前
- OPTIONS 请求（预检）会被 Referer 验证跳过
- 实际请求会同时经过 CORS 和 Referer 验证

## 常见问题

### Q: 为什么修改配置后不生效？

A: 必须重启服务。ASP.NET Core 在启动时读取配置，运行时不会自动重新加载。

### Q: 为什么浏览器还能访问？

A: 可能是浏览器缓存。尝试：
1. 清除缓存
2. 使用无痕模式
3. 硬刷新（Ctrl+Shift+R）

### Q: Swagger 还能访问吗？

A: 可以。Swagger 端点被跳过验证。

### Q: 如何完全禁止外部访问？

A: 将 `AllowedOrigins` 设置为空数组：
```json
{
  "AllowedOrigins": []
}
```

但这样连 TtsWebApp 也无法访问了。

## 验证成功的标志

✅ **配置生效的标志：**

1. 启动日志只显示生产域名
2. 从 localhost:5000 访问 API 返回 403
3. 浏览器控制台显示 CORS 错误
4. API 日志显示 "未授权的来源"

❌ **配置未生效的标志：**

1. 启动日志包含 localhost
2. 从 localhost:5000 可以正常访问
3. 没有任何错误信息

---

**最后更新：** 2025年1月1日
