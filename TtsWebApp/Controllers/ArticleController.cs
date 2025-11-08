using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtsWebApp.Data;
using TtsWebApp.Models;

namespace TtsWebApp.Controllers;

public class ArticleController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ArticleController> _logger;
    
    public ArticleController(AppDbContext context, ILogger<ArticleController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // 文章列表页
    public async Task<IActionResult> Index(string lang = "zh-CN", int page = 1, int pageSize = 10)
    {
        // 从 localStorage 或 query 参数获取语言
        var currentLang = lang ?? "zh-CN";
        
        var query = _context.Articles
            .Where(a => a.IsPublished 
                     && a.Type == ArticleType.Article 
                     && a.Language == currentLang)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);
        
        var total = await query.CountAsync();
        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.CurrentLang = currentLang;
        
        return View(articles);
    }
    
    // 文章详情页
    public async Task<IActionResult> Detail(int id)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished && a.Type == ArticleType.Article);
        
        if (article == null)
        {
            return NotFound();
        }
        
        // 查找关联的其他语言版本
        Article? relatedArticle = null;
        if (article.RelatedArticleId.HasValue)
        {
            relatedArticle = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == article.RelatedArticleId.Value && a.IsPublished);
        }
        
        // 增加浏览量
        article.ViewCount++;
        await _context.SaveChangesAsync();
        
        ViewBag.RelatedArticle = relatedArticle;
        
        return View(article);
    }
    
    // 获取最新文章（用于首页）
    public async Task<IActionResult> Latest(int count = 5)
    {
        var articles = await _context.Articles
            .Where(a => a.IsPublished && a.Type == ArticleType.Article)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(count)
            .ToListAsync();
        
        return PartialView("_LatestArticles", articles);
    }
}
