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
            // 只记录 POST 请求到 synthesize
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
                
                _logger.LogInformation("收到 POST /api/tts/synthesize 请求");
                _logger.LogInformation("请求体长度: {Length}", body.Length);
                _logger.LogInformation("请求体内容: {Body}", body.Length > 1000 ? body.Substring(0, 1000) + "..." : body);
            }

            await _next(context);
        }
    }
}
