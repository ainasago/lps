using TtsWebApi.Services;
using TtsWebApi.Controllers;
using TtsWebApi.Middleware;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

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
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[]
{
    "localhost:5000",
    "localhost:5001"
};

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

Console.WriteLine($"CORS 允许的来源: {string.Join(", ", allowedUrls)}");

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

app.Run();
