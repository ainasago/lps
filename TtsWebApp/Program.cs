using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using TtsWebApp.Data;
using TtsWebApp.Services;
using TtsWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tts}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
