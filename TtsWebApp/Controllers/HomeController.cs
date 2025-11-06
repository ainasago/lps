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
