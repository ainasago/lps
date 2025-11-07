using System.Text;

namespace TtsWebApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            // 记录所有请求的基本信息
            _logger.LogInformation("[{RequestId}] {Method} {Path} - 来自 {RemoteIp}", 
                requestId,
                context.Request.Method, 
                context.Request.Path,
                context.Connection.RemoteIpAddress);
            
            // 详细记录 POST 请求到 synthesize
            if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/tts/synthesize"))
            {
                context.Request.EnableBuffering();
                
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                
                _logger.LogInformation("[{RequestId}] 请求体长度: {Length} 字节", requestId, body.Length);
                
                // 记录 Referer 和 User-Agent
                var referer = context.Request.Headers["Referer"].ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    _logger.LogInformation("[{RequestId}] Referer: {Referer}", requestId, referer);
                }
                if (!string.IsNullOrEmpty(userAgent))
                {
                    _logger.LogInformation("[{RequestId}] User-Agent: {UserAgent}", requestId, userAgent);
                }
            }

            try
            {
                await _next(context);
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("[{RequestId}] {Method} {Path} - 状态码: {StatusCode}, 耗时: {Duration}ms", 
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "[{RequestId}] {Method} {Path} - 发生异常, 耗时: {Duration}ms", 
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    duration);
                throw;
            }
        }
    }
}
