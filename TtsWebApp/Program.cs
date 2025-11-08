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
        
        // 读取并添加页面（支持中英文）
        var pageFiles = new Dictionary<string, (string titleZh, string titleEn, string slug)>
        {
            ["about"] = ("关于我们", "About Us", "about"),
            ["privacy"] = ("隐私政策", "Privacy Policy", "privacy"),
            ["terms"] = ("服务条款", "Terms of Service", "terms"),
            ["disclaimer"] = ("免责声明", "Disclaimer", "disclaimer"),
            ["contact"] = ("联系我们", "Contact Us", "contact")
        };
        
        foreach (var (key, (titleZh, titleEn, slug)) in pageFiles)
        {
            // 读取中文版本
            var filePathZh = Path.Combine(dataPath, $"init_{key}_zh.txt");
            var filePathEn = Path.Combine(dataPath, $"init_{key}_en.txt");
            
            string contentZh = "";
            string contentEn = "";
            
            // 尝试读取中文文件
            if (File.Exists(filePathZh))
            {
                contentZh = File.ReadAllText(filePathZh);
                logger.LogInformation("成功读取中文文件: {FileName}, 内容长度: {Length}", $"init_{key}_zh.txt", contentZh.Length);
            }
            else
            {
                // 兼容旧文件名（无语言后缀）
                var oldFilePath = Path.Combine(dataPath, $"init_{key}.txt");
                if (File.Exists(oldFilePath))
                {
                    contentZh = File.ReadAllText(oldFilePath);
                    logger.LogInformation("使用旧格式文件: {FileName}", $"init_{key}.txt");
                }
                else
                {
                    contentZh = $"<p>{titleZh}页面内容</p>";
                    logger.LogWarning("中文文件不存在: {FilePath}, 使用默认内容", filePathZh);
                }
            }
            
            // 尝试读取英文文件
            if (File.Exists(filePathEn))
            {
                contentEn = File.ReadAllText(filePathEn);
                logger.LogInformation("成功读取英文文件: {FileName}, 内容长度: {Length}", $"init_{key}_en.txt", contentEn.Length);
            }
            else
            {
                contentEn = $"<p>{titleEn} page content</p>";
                logger.LogWarning("英文文件不存在: {FilePath}, 使用默认内容", filePathEn);
            }
            
            // 添加中文页面
            pages.Add(new Article
            {
                Title = titleZh,
                Slug = slug,
                Content = contentZh,
                Type = ArticleType.Page,
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now
            });
            
            // 添加英文页面
            pages.Add(new Article
            {
                Title = titleEn,
                Slug = $"{slug}-en",
                Content = contentEn,
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
