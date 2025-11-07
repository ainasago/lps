namespace TtsWebApi.Middleware;

/// <summary>
/// Referer 验证中间件 - 确保请求来自允许的来源
/// </summary>
public class RefererValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RefererValidationMiddleware> _logger;
    private readonly HashSet<string> _allowedHosts;

    public RefererValidationMiddleware(
        RequestDelegate next, 
        ILogger<RefererValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        
        // 从配置读取允许的主机
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
        
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            _logger.LogWarning("警告：未配置 AllowedOrigins，Referer 验证将拒绝所有请求！请在 appsettings.json 中配置 AllowedOrigins");
            _allowedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            _allowedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // 处理配置中的每个来源，提取 host:port 格式
            foreach (var origin in allowedOrigins)
            {
                try
                {
                    // 如果包含协议，解析 URL
                    if (origin.StartsWith("http://") || origin.StartsWith("https://"))
                    {
                        var uri = new Uri(origin);
                        var host = uri.Host;
                        if (uri.Port != 80 && uri.Port != 443)
                        {
                            host = $"{uri.Host}:{uri.Port}";
                        }
                        _allowedHosts.Add(host);
                    }
                    else
                    {
                        // 直接使用 host:port 格式
                        _allowedHosts.Add(origin);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析允许的来源失败: {Origin}", origin);
                }
            }
            
            _logger.LogInformation("Referer 验证已启用，允许的主机: {Hosts}", string.Join(", ", _allowedHosts));
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 跳过 OPTIONS 请求（CORS 预检）
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }
        
        // 跳过 Swagger 和健康检查端点
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        // 只验证 API 端点
        if (path.StartsWith("/api/"))
        {
            var referer = context.Request.Headers["Referer"].ToString();
            var origin = context.Request.Headers["Origin"].ToString();
            
            // 检查 Referer 或 Origin
            var sourceUrl = !string.IsNullOrEmpty(referer) ? referer : origin;
            
            if (string.IsNullOrEmpty(sourceUrl))
            {
                _logger.LogWarning("API 请求缺少 Referer 和 Origin 头: {Path} from {IP}", 
                    context.Request.Path, 
                    context.Connection.RemoteIpAddress);
                
                // 拒绝所有缺少来源信息的请求
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Missing request source");
                return;
            }
            else
            {
                // 验证来源是否在允许列表中
                var isAllowed = false;
                try
                {
                    var uri = new Uri(sourceUrl);
                    var host = uri.Host;
                    if (uri.Port != 80 && uri.Port != 443)
                    {
                        host = $"{uri.Host}:{uri.Port}";
                    }
                    
                    isAllowed = _allowedHosts.Contains(host);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "解析来源 URL 失败: {SourceUrl}", sourceUrl);
                }

                if (!isAllowed)
                {
                    _logger.LogWarning("API 请求来自未授权的来源: {Source} for {Path}", 
                        sourceUrl, 
                        context.Request.Path);
                    
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden: Unauthorized source");
                    return;
                }
                
                _logger.LogInformation("API 请求来自授权来源: {Source}", sourceUrl);
            }
        }

        await _next(context);
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class RefererValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRefererValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RefererValidationMiddleware>();
    }
}
