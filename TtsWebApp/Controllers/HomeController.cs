using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtsWebApp.Data;
using TtsWebApp.Models;

namespace TtsWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Privacy()
    {
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == "privacy" && a.Type == ArticleType.Page);
        return View(page);
    }

    public async Task<IActionResult> About()
    {
        _logger.LogInformation("查询关于我们页面...");
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == "about" && a.Type == ArticleType.Page);
        
        if (page == null)
        {
            _logger.LogWarning("未找到关于我们页面 (slug: about)");
            var allPages = await _context.Articles.Where(a => a.Type == ArticleType.Page).ToListAsync();
            _logger.LogInformation("数据库中的页面: {Pages}", string.Join(", ", allPages.Select(p => $"{p.Slug}({p.Title})")));
        }
        else
        {
            _logger.LogInformation("找到页面: {Title}, 内容长度: {Length}", page.Title, page.Content?.Length ?? 0);
        }
        
        return View(page);
    }

    public async Task<IActionResult> Contact()
    {
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == "contact" && a.Type == ArticleType.Page);
        return View(page);
    }

    public async Task<IActionResult> Terms()
    {
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == "terms" && a.Type == ArticleType.Page);
        return View(page);
    }

    public async Task<IActionResult> Disclaimer()
    {
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == "disclaimer" && a.Type == ArticleType.Page);
        return View(page);
    }
    
    // 通用页面路由处理（通过 slug 访问）
    // Order = 999 确保这个路由最后匹配，不会干扰其他控制器路由
    [Route("/{slug}", Order = 999)]
    public async Task<IActionResult> Page(string slug)
    {
        // 排除空 slug（根路径）
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }
        
        // 排除包含文件扩展名的请求（让静态文件正常访问）
        if (slug.Contains('.'))
        {
            return NotFound();
        }
        
        // 检查是否是后台路径（包括子路径）
        var adminPath = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["AppSettings:AdminPath"] ?? "Admin";
        if (slug.Equals(adminPath, StringComparison.OrdinalIgnoreCase) || 
            Request.Path.StartsWithSegments($"/{adminPath}", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }
        
        _logger.LogInformation("访问页面 slug: {Slug}", slug);
        
        var page = await _context.Articles
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Type == ArticleType.Page && a.IsPublished);
        
        if (page == null)
        {
            _logger.LogWarning("未找到页面: {Slug}", slug);
            return NotFound();
        }
        
        _logger.LogInformation("找到页面: {Title}", page.Title);
        
        // 使用通用的页面视图
        return View("PageView", page);
    }

    public IActionResult Features()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
