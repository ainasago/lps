using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using TtsWebApp.Data;
using TtsWebApp.Services;
using TtsWebApp.Models;
using WebOptimizer;
using Serilog;
using Serilog.Events;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("应用程序启动中...");

var builder = WebApplication.CreateBuilder(args);

// 使用 Serilog
builder.Host.UseSerilog();

// Add services to the container.
// 启用 Razor 运行时编译，允许在发布后修改视图文件
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();

// 添加 WebOptimizer（JS/CSS 压缩和混淆）
// WebOptimizer 会自动压缩和混淆 JS/CSS 文件
builder.Services.AddWebOptimizer(pipeline =>
{
    // 压缩和混淆所有 JS 文件（包括 tts.js）
    pipeline.MinifyJsFiles("js/**/*.js");
    pipeline.MinifyCssFiles("css/**/*.css");
});

// 添加数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加身份验证
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = $"/{builder.Configuration["AppSettings:AdminPath"]}/Login";
        options.LogoutPath = $"/{builder.Configuration["AppSettings:AdminPath"]}/Logout";
        options.AccessDeniedPath = $"/{builder.Configuration["AppSettings:AdminPath"]}/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// 添加配置服务
builder.Services.AddScoped<IConfigService, ConfigService>();

// 添加文件管理服务
builder.Services.Configure<FileManagerConfig>(builder.Configuration.GetSection("FileManager"));
builder.Services.AddScoped<IFileManagerService, FileManagerService>();

var app = builder.Build();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("开始初始化数据库...");
    db.Database.EnsureCreated();
    
    // 检查是否需要初始化页面数据
    if (!db.Articles.Any(a => a.Type == ArticleType.Page))
    {
        logger.LogInformation("检测到没有页面数据，开始初始化...");
        
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        logger.LogInformation("数据文件路径: {DataPath}", dataPath);
        
        var now = DateTime.Now;
        var pages = new List<Article>();
        
        // 读取并添加页面
        var pageFiles = new Dictionary<string, (string title, string slug)>
        {
            ["init_about.txt"] = ("关于我们", "about"),
            ["init_privacy.txt"] = ("隐私政策", "privacy"),
            ["init_terms.txt"] = ("服务条款", "terms"),
            ["init_disclaimer.txt"] = ("免责声明", "disclaimer"),
            ["init_contact.txt"] = ("联系我们", "contact")
        };
        
        foreach (var (fileName, (title, slug)) in pageFiles)
        {
            var filePath = Path.Combine(dataPath, fileName);
            string content;
            
            if (File.Exists(filePath))
            {
                content = File.ReadAllText(filePath);
                logger.LogInformation("成功读取文件: {FileName}, 内容长度: {Length}", fileName, content.Length);
            }
            else
            {
                content = $"<p>{title}页面内容</p>";
                logger.LogWarning("文件不存在: {FilePath}, 使用默认内容", filePath);
            }
            
            pages.Add(new Article
            {
                Title = title,
                Slug = slug,
                Content = content,
                Type = ArticleType.Page,
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now
            });
        }
        
        db.Articles.AddRange(pages);
        db.SaveChanges();
        
        logger.LogInformation("成功初始化 {Count} 个页面", pages.Count);
    }
    else
    {
        logger.LogInformation("页面数据已存在，跳过初始化");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// 使用 WebOptimizer（必须在 UseStaticFiles 之前）
app.UseWebOptimizer();

// 启用静态文件访问
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 动态后台路由
var adminPath = app.Configuration["AppSettings:AdminPath"] ?? "Admin";
app.Logger.LogInformation("后台管理路径: /{AdminPath}", adminPath);

// FileManager 专用路由
app.MapControllerRoute(
    name: "filemanager",
    pattern: $"{adminPath}/FileManager/{{action=Index}}",
    defaults: new { controller = "FileManager", action = "Index" });

// Admin 专用路由
app.MapControllerRoute(
    name: "admin",
    pattern: $"{adminPath}/{{action=Index}}/{{id?}}",
    defaults: new { controller = "Admin" });

// 默认路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tts}/{action=Index}/{id?}");

app.MapStaticAssets();

    Log.Information("应用程序启动完成");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
