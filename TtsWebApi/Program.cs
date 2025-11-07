using TtsWebApi.Services;
using TtsWebApi.Controllers;
using TtsWebApi.Middleware;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/tts-api-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10485760) // 10MB
    .CreateLogger();

try
{
    Log.Information("=== TTS API 服务启动中 ===");

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient
builder.Services.AddHttpClient();

// Register TTS Service
builder.Services.AddScoped<ITtsService, TtsService>();

// Register advanced features
builder.Services.AddScoped<TextSplitter>();
builder.Services.AddScoped<MultiRoleParser>();
builder.Services.AddScoped<AudioMerger>();

// 从配置文件读取允许的来源
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    Log.Warning("警告：未配置 AllowedOrigins，CORS 将不允许任何跨域请求！请在 appsettings.json 中配置 AllowedOrigins");
    allowedOrigins = Array.Empty<string>();
}

// 构建完整的 URL（添加 http:// 和 https://）
var allowedUrls = new List<string>();
foreach (var origin in allowedOrigins)
{
    // 如果已经包含协议，直接添加
    if (origin.StartsWith("http://") || origin.StartsWith("https://"))
    {
        allowedUrls.Add(origin);
    }
    else
    {
        // 否则添加 http 和 https 两个版本
        allowedUrls.Add($"http://{origin}");
        allowedUrls.Add($"https://{origin}");
    }
}

if (allowedUrls.Count > 0)
{
    Log.Information("CORS 允许的来源: {Origins}", string.Join(", ", allowedUrls));
}
else
{
    Log.Warning("CORS 未配置任何允许的来源");
}

// Add CORS - 只允许配置的来源调用
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTtsWebApp", policy =>
    {
        policy.WithOrigins(allowedUrls.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // 允许携带凭证
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable static files
app.UseStaticFiles();

// CORS 必须在 Referer 验证之前，以便处理预检请求
app.UseCors("AllowTtsWebApp");

// 启用 Referer 验证中间件（在 CORS 之后）
app.UseRefererValidation();

// 添加请求日志中间件
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

Log.Information("=== TTS API 服务已启动，正在监听请求 ===");
app.Run();
Log.Information("=== TTS API 服务已停止 ===");
}
catch (Exception ex)
{
    Log.Fatal(ex, "TTS API 服务启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
