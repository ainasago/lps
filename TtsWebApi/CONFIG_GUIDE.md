# API 访问控制配置指南

## 问题说明

之前的配置存在一个问题：

- **CORS 策略** 在 `Program.cs` 中硬编码了允许的来源
- **Referer 验证** 从 `appsettings.json` 读取配置

这导致即使修改了 `appsettings.json`，CORS 策略仍然允许硬编码的来源访问。

## 解决方案

现在**两个安全机制都从配置文件读取**，统一管理允许的来源。

## 配置文件

### 1. appsettings.json（开发环境）

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

### 2. appsettings.Production.json（生产环境）

```json
{
  "AllowedOrigins": [
    "https://www.annietts.com",
    "https://annietts.com"
  ]
}
```

**注意：** 生产环境配置会覆盖开发环境配置。

## 配置格式

### 支持的格式

1. **不带协议**（推荐）
   ```json
   "localhost:5000"
   ```
   系统会自动添加 `http://` 和 `https://` 两个版本

2. **带协议**
   ```json
   "https://www.annietts.com"
   ```
   系统会直接使用指定的协议

### 示例

**配置：**
```json
{
  "AllowedOrigins": [
    "localhost:5000",
    "https://www.annietts.com"
  ]
}
```

**实际生效的 URL：**
- `http://localhost:5000`
- `https://localhost:5000`
- `https://www.annietts.com`

## 测试方法

### 1. 查看启动日志

启动 API 时，控制台会输出允许的来源：

```
CORS 允许的来源: http://localhost:5000, https://localhost:5000, https://www.annietts.com, https://annietts.com
```

### 2. 测试允许的来源

```bash
# 应该成功（如果 localhost:5000 在配置中）
curl -H "Origin: http://localhost:5000" \
     http://localhost:5001/api/tts/voices
```

### 3. 测试被拒绝的来源

```bash
# 应该失败（403 Forbidden）
curl -H "Origin: http://evil.com" \
     http://localhost:5001/api/tts/voices
```

## 修改配置

### 开发环境

直接修改 `appsettings.json`：

```json
{
  "AllowedOrigins": [
    "localhost:5000",
    "localhost:8080"  // 添加新端口
  ]
}
```

重启 API 服务即可生效。

### 生产环境

修改 `appsettings.Production.json`：

```json
{
  "AllowedOrigins": [
    "https://www.annietts.com",
    "https://api.annietts.com"  // 添加新域名
  ]
}
```

部署时确保使用生产环境配置：

```bash
dotnet run --environment Production
```

或设置环境变量：

```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## 安全建议

### ✅ 推荐做法

1. **最小权限原则**
   - 只添加必要的来源
   - 生产环境不要包含 localhost

2. **使用 HTTPS**
   - 生产环境只允许 HTTPS
   - 例如：`https://www.annietts.com`

3. **定期审查**
   - 定期检查配置文件
   - 删除不再使用的来源

### ❌ 不推荐做法

1. **不要在生产环境允许 localhost**
   ```json
   // ❌ 错误
   {
     "AllowedOrigins": [
       "localhost:5000",
       "www.annietts.com"
     ]
   }
   ```

2. **不要使用通配符**
   ```json
   // ❌ 错误
   {
     "AllowedOrigins": ["*"]
   }
   ```

3. **不要允许 HTTP（生产环境）**
   ```json
   // ❌ 错误（生产环境）
   {
     "AllowedOrigins": [
       "http://www.annietts.com"
     ]
   }
   ```

## 故障排除

### 问题 1：修改配置后不生效

**原因：** 没有重启服务

**解决：** 重启 API 服务

### 问题 2：CORS 错误

**症状：**
```
Access to fetch at 'http://localhost:5001/api/tts/voices' from origin 'http://localhost:5000' 
has been blocked by CORS policy
```

**检查步骤：**
1. 查看启动日志，确认允许的来源
2. 检查 `appsettings.json` 中的配置
3. 确认请求的 Origin 是否在允许列表中

**解决：**
```json
{
  "AllowedOrigins": [
    "localhost:5000"  // 添加这个
  ]
}
```

### 问题 3：403 Forbidden

**症状：**
```
403 Forbidden: Unauthorized source
```

**原因：** Referer 验证失败

**检查步骤：**
1. 查看 API 日志
2. 确认请求包含 Referer 或 Origin 头
3. 确认来源在配置文件中

**解决：**
确保配置文件包含正确的来源。

## 配置优先级

ASP.NET Core 配置优先级（从高到低）：

1. 命令行参数
2. 环境变量
3. `appsettings.{Environment}.json`
4. `appsettings.json`

**示例：**

开发环境会合并：
- `appsettings.json`
- `appsettings.Development.json`（如果存在）

生产环境会合并：
- `appsettings.json`
- `appsettings.Production.json`（覆盖 appsettings.json）

## 快速配置模板

### 开发环境（appsettings.json）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AllowedOrigins": [
    "localhost:5000",
    "localhost:5001",
    "localhost:7000",
    "localhost:7001"
  ]
}
```

### 生产环境（appsettings.Production.json）

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedOrigins": [
    "https://www.annietts.com",
    "https://annietts.com"
  ]
}
```

## 总结

现在修改 `appsettings.json` 中的 `AllowedOrigins` 会**同时影响**：

1. ✅ CORS 策略
2. ✅ Referer 验证中间件

这样配置更加统一和安全！

---

**最后更新：** 2025年1月1日  
**版本：** 2.0
