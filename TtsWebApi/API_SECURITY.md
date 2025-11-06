# TtsWebApi 安全配置说明

## 概述

TtsWebApi 已配置为只允许来自 TtsWebApp 的请求，通过以下两层安全机制实现：

1. **CORS 策略** - 限制跨域请求来源
2. **Referer 验证中间件** - 验证请求来源的 Referer/Origin 头

## 1. CORS 策略配置

### 配置位置
- 文件：`Program.cs`
- 策略名称：`AllowTtsWebApp`

### 允许的来源

**开发环境：**
- `http://localhost:5000` - TtsWebApp 默认端口
- `https://localhost:5001` - TtsWebApp HTTPS 端口
- `http://localhost:7000` - TtsWebApp 备用端口
- `https://localhost:7001` - TtsWebApp 备用端口 HTTPS

**生产环境：**
- `https://www.annietts.com` - 生产域名（带 www）
- `https://annietts.com` - 生产域名（不带 www）

### 配置代码

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTtsWebApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:5001",
                "http://localhost:7000",
                "https://localhost:7001",
                "https://www.annietts.com",
                "https://annietts.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## 2. Referer 验证中间件

### 功能说明

验证每个 API 请求的 `Referer` 或 `Origin` 头，确保请求来自允许的来源。

### 配置位置
- 文件：`Middleware/RefererValidationMiddleware.cs`
- 配置文件：`appsettings.json`

### 验证逻辑

1. **跳过验证的端点：**
   - `/swagger/*` - Swagger 文档
   - `/health` - 健康检查

2. **API 端点验证：**
   - 检查 `Referer` 头
   - 如果没有 `Referer`，检查 `Origin` 头
   - 验证来源是否在允许列表中

3. **开发环境特殊处理：**
   - 开发环境允许缺少 Referer/Origin 的请求
   - 生产环境拒绝缺少 Referer/Origin 的请求

### 配置文件

`appsettings.json`:
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

### 响应状态码

- **403 Forbidden** - 请求来自未授权的来源
- **403 Forbidden** - 生产环境缺少 Referer/Origin 头

## 3. 中间件管道顺序

```
请求进入
    ↓
UseHttpsRedirection
    ↓
UseStaticFiles
    ↓
UseRefererValidation  ← 验证来源
    ↓
UseCors               ← CORS 策略
    ↓
UseAuthorization
    ↓
MapControllers
```

**重要：** `UseRefererValidation` 必须在 `UseCors` 之前调用。

## 4. 日志记录

### 记录的信息

1. **警告日志：**
   - 缺少 Referer/Origin 头的请求
   - 来自未授权来源的请求

2. **信息日志：**
   - 来自授权来源的请求

3. **错误日志：**
   - URL 解析失败

### 日志示例

```
[Warning] API 请求缺少 Referer 和 Origin 头: /api/tts/convert from 127.0.0.1
[Warning] API 请求来自未授权的来源: http://evil.com for /api/tts/convert
[Information] API 请求来自授权来源: http://localhost:5000
```

## 5. 部署配置

### 添加新的允许来源

**方法 1：修改 appsettings.json**

```json
{
  "AllowedOrigins": [
    "localhost:5000",
    "your-new-domain.com"
  ]
}
```

**方法 2：修改 Program.cs**

```csharp
policy.WithOrigins(
    "http://localhost:5000",
    "https://your-new-domain.com"
)
```

### 生产环境部署清单

- [ ] 更新 `appsettings.json` 中的 `AllowedOrigins`
- [ ] 更新 `Program.cs` 中的 CORS 策略
- [ ] 确保使用 HTTPS
- [ ] 测试 CORS 策略
- [ ] 测试 Referer 验证
- [ ] 检查日志输出

## 6. 测试

### 测试 CORS 策略

**允许的请求：**
```bash
curl -H "Origin: http://localhost:5000" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS \
     http://localhost:5001/api/tts/voices
```

**预期响应：**
- 状态码：200
- 包含 `Access-Control-Allow-Origin` 头

**被拒绝的请求：**
```bash
curl -H "Origin: http://evil.com" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS \
     http://localhost:5001/api/tts/voices
```

**预期响应：**
- 状态码：403 或没有 CORS 头

### 测试 Referer 验证

**允许的请求：**
```bash
curl -H "Referer: http://localhost:5000/" \
     http://localhost:5001/api/tts/voices
```

**预期响应：**
- 状态码：200
- 返回语音列表

**被拒绝的请求：**
```bash
curl -H "Referer: http://evil.com/" \
     http://localhost:5001/api/tts/voices
```

**预期响应：**
- 状态码：403
- 消息：`Forbidden: Unauthorized source`

## 7. 故障排除

### 问题 1：CORS 错误

**症状：**
```
Access to fetch at 'http://localhost:5001/api/tts/voices' from origin 'http://localhost:5000' 
has been blocked by CORS policy
```

**解决方案：**
1. 检查 `Program.cs` 中的 CORS 策略
2. 确保 TtsWebApp 的 URL 在允许列表中
3. 检查 `UseCors` 的调用顺序

### 问题 2：403 Forbidden

**症状：**
```
403 Forbidden: Unauthorized source
```

**解决方案：**
1. 检查 `appsettings.json` 中的 `AllowedOrigins`
2. 确保请求包含正确的 `Referer` 或 `Origin` 头
3. 检查日志查看详细错误信息

### 问题 3：开发环境无法访问

**症状：**
开发环境也返回 403 错误

**解决方案：**
1. 确认当前环境是 Development
2. 检查 `launchSettings.json` 中的环境变量
3. 临时禁用 Referer 验证进行测试

## 8. 安全建议

### 推荐做法

1. **使用 HTTPS**
   - 生产环境必须使用 HTTPS
   - 防止中间人攻击

2. **定期更新允许列表**
   - 只添加必要的来源
   - 定期审查允许列表

3. **监控日志**
   - 定期检查未授权访问尝试
   - 设置告警机制

4. **API 密钥（可选）**
   - 考虑添加 API 密钥验证
   - 进一步增强安全性

### 不推荐做法

1. ❌ 使用 `AllowAnyOrigin()`
2. ❌ 在生产环境禁用 Referer 验证
3. ❌ 将 API 暴露在公网而不加保护
4. ❌ 忽略安全日志

## 9. 性能影响

### Referer 验证中间件

- **性能开销：** 极小（< 1ms）
- **内存开销：** 极小（HashSet 查找）
- **建议：** 可以安全地在生产环境使用

### CORS 策略

- **性能开销：** 极小
- **缓存：** 浏览器会缓存 CORS 预检请求
- **建议：** 标准做法，必须启用

## 10. 相关文件

- `TtsWebApi/Program.cs` - CORS 配置
- `TtsWebApi/Middleware/RefererValidationMiddleware.cs` - Referer 验证
- `TtsWebApi/appsettings.json` - 允许来源配置
- `TtsWebApp/Controllers/TtsController.cs` - API 调用客户端

## 联系方式

如有问题或建议，请联系：contact@annietts.com

---

**最后更新：** 2025年1月1日  
**版本：** 1.0
