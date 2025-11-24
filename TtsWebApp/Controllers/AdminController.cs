using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TtsWebApp.Data;
using TtsWebApp.Models;
using TtsWebApp.Resources;
using TtsWebApp.Services;

namespace TtsWebApp.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfigService _configService;
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _configuration;
    
    public AdminController(
        AppDbContext context, 
        IConfigService configService,
        ILogger<AdminController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _configService = configService;
        _logger = logger;
        _configuration = configuration;
    }
    
    // 登录页面
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    // 登录处理
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "用户名和密码不能为空";
            return View();
        }
        
        var user = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username);
        
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            ViewBag.Error = "用户名或密码错误";
            return View();
        }
        
        // 更新最后登录时间
        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();
        
        // 创建身份验证票据
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
        
        _logger.LogInformation("用户 {Username} 登录成功", username);
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    // 退出登录
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
    
    // 管理后台首页
    public async Task<IActionResult> Index()
    {
        ViewBag.ArticleCount = await _context.Articles.CountAsync(a => a.Type == ArticleType.Article);
        ViewBag.PublishedCount = await _context.Articles.CountAsync(a => a.Type == ArticleType.Article && a.IsPublished);
        ViewBag.PageCount = await _context.Articles.CountAsync(a => a.Type == ArticleType.Page);
        ViewBag.TtsRecordCount = await _context.TtsConversionRecords.CountAsync();
        return View();
    }
    
    // 网站配置管理
    public async Task<IActionResult> SiteConfig()
    {
        var configs = await _context.SiteConfigs.OrderBy(c => c.Id).ToListAsync();
        return View(configs);
    }
    
    // 更新网站配置
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSiteConfig(Dictionary<string, string> configs)
    {
        try
        {
            foreach (var kvp in configs)
            {
                await _configService.SetConfigAsync(kvp.Key, kvp.Value);
            }
            
            TempData["Success"] = "配置更新成功";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配置失败");
            TempData["Error"] = "配置更新失败";
        }
        
        return RedirectToAction(nameof(SiteConfig));
    }
    
    // 文章列表
    public async Task<IActionResult> Articles(int page = 1, int pageSize = 20)
    {
        var query = _context.Articles
            .Where(a => a.Type == ArticleType.Article)
            .OrderByDescending(a => a.CreatedAt);
        var total = await query.CountAsync();
        var articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        
        return View(articles);
    }
    
    // 页面管理
    public async Task<IActionResult> Pages()
    {
        var pages = await _context.Articles
            .Where(a => a.Type == ArticleType.Page)
            .OrderBy(a => a.Id)
            .ToListAsync();
        
        return View(pages);
    }
    
    // 编辑页面
    public async Task<IActionResult> EditPage(int id)
    {
        var page = await _context.Articles.FindAsync(id);
        if (page == null || page.Type != ArticleType.Page)
        {
            return NotFound();
        }
        
        return View(page);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPage(int id, Article page)
    {
        if (id != page.Id)
        {
            return NotFound();
        }
        
        var existingPage = await _context.Articles.FindAsync(id);
        if (existingPage == null || existingPage.Type != ArticleType.Page)
        {
            return NotFound();
        }
        
        existingPage.Title = page.Title;
        existingPage.Content = page.Content;
        existingPage.UpdatedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "页面更新成功";
        return RedirectToAction(nameof(Pages));
    }
    
    // 创建文章
    public IActionResult CreateArticle()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArticle(Article article)
    {
        if (!ModelState.IsValid)
        {
            return View(article);
        }
        
        article.CreatedAt = DateTime.Now;
        article.UpdatedAt = DateTime.Now;
        
        if (article.IsPublished)
        {
            article.PublishedAt = DateTime.Now;
        }
        
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "文章创建成功";
        return RedirectToAction(nameof(Articles));
    }
    
    // 编辑文章
    public async Task<IActionResult> EditArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }
        
        return View(article);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditArticle(int id, Article article)
    {
        if (id != article.Id)
        {
            return NotFound();
        }
        
        if (!ModelState.IsValid)
        {
            return View(article);
        }
        
        var existingArticle = await _context.Articles.FindAsync(id);
        if (existingArticle == null)
        {
            return NotFound();
        }
        
        existingArticle.Title = article.Title;
        existingArticle.Summary = article.Summary;
        existingArticle.Content = article.Content;
        existingArticle.Author = article.Author;
        existingArticle.Tags = article.Tags;
        existingArticle.Language = article.Language;
        existingArticle.RelatedArticleId = article.RelatedArticleId;
        existingArticle.UpdatedAt = DateTime.Now;
        
        if (article.IsPublished && !existingArticle.IsPublished)
        {
            existingArticle.PublishedAt = DateTime.Now;
        }
        existingArticle.IsPublished = article.IsPublished;
        
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "文章更新成功";
        return RedirectToAction(nameof(Articles));
    }
    
    // 删除文章
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }
        
        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "文章删除成功";
        return RedirectToAction(nameof(Articles));
    }
    
    // 账户设置页面
    public async Task<IActionResult> AccountSettings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.AdminUsers.FindAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }
        
        return View(user);
    }
    
    // 修改用户名
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUsername(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
        {
            TempData["Error"] = "用户名不能为空";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        if (newUsername.Length < 3 || newUsername.Length > 50)
        {
            TempData["Error"] = "用户名长度必须在 3-50 个字符之间";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.AdminUsers.FindAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }
        
        // 检查用户名是否已存在
        var existingUser = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == newUsername && u.Id != userId);
        
        if (existingUser != null)
        {
            TempData["Error"] = "用户名已存在";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        var oldUsername = user.Username;
        user.Username = newUsername;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("用户 {OldUsername} 修改用户名为 {NewUsername}", oldUsername, newUsername);
        
        // 更新登录状态
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };
        
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
        
        TempData["Success"] = "用户名修改成功";
        return RedirectToAction(nameof(AccountSettings));
    }
    
    // 修改密码
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Error"] = "密码不能为空";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "两次输入的新密码不一致";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        if (newPassword.Length < 6)
        {
            TempData["Error"] = "密码长度至少为 6 个字符";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.AdminUsers.FindAsync(userId);
        
        if (user == null)
        {
            return NotFound();
        }
        
        // 验证当前密码
        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            TempData["Error"] = "当前密码错误";
            return RedirectToAction(nameof(AccountSettings));
        }
        
        // 更新密码
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("用户 {Username} 修改了密码", user.Username);
        
        TempData["Success"] = "密码修改成功";
        return RedirectToAction(nameof(AccountSettings));
    }
    
    // 简单的密码验证（实际应使用 BCrypt）
    private bool VerifyPassword(string password, string hash)
    {
        // 临时简单验证，实际应使用 BCrypt.Net.BCrypt.Verify
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    // ==================== TTS 转换记录管理 ====================
    
    // TTS 记录列表
    public async Task<IActionResult> TtsRecords(int page = 1, string? search = null, int pageSize = 20)
    {
        var query = _context.TtsConversionRecords.AsQueryable();
        
        // 搜索
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Text.Contains(search) || 
                                    r.IpAddress.Contains(search) ||
                                    r.Language.Contains(search) ||
                                    r.Voice.Contains(search));
        }
        
        // 总数
        var total = await query.CountAsync();
        
        // 分页
        var records = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Search = search;
        
        return View(records);
    }
    
    // 查看 TTS 记录详情
    public async Task<IActionResult> TtsRecordDetail(int id)
    {
        var record = await _context.TtsConversionRecords.FindAsync(id);
        
        if (record == null)
        {
            return NotFound();
        }
        
        return View(record);
    }
    
    // 删除 TTS 记录
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTtsRecord(int id)
    {
        var record = await _context.TtsConversionRecords.FindAsync(id);
        
        if (record == null)
        {
            return NotFound();
        }
        
        _context.TtsConversionRecords.Remove(record);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "记录已删除";
        return RedirectToAction(nameof(TtsRecords));
    }
    
    // 批量删除 TTS 记录
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchDeleteTtsRecords(int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            TempData["Error"] = "请选择要删除的记录";
            return RedirectToAction(nameof(TtsRecords));
        }
        
        var records = await _context.TtsConversionRecords
            .Where(r => ids.Contains(r.Id))
            .ToListAsync();
        
        _context.TtsConversionRecords.RemoveRange(records);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = $"已删除 {records.Count} 条记录";
        return RedirectToAction(nameof(TtsRecords));
    }
    
    // TTS 数据统计
    public async Task<IActionResult> TtsStatistics()
    {
        // 获取最近30天的数据
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var records = await _context.TtsConversionRecords
            .Where(r => r.CreatedAt >= thirtyDaysAgo)
            .ToListAsync();
        
        // 每日转换量统计
        var dailyStats = records
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count(),
                SuccessCount = g.Count(r => r.IsSuccess),
                FailCount = g.Count(r => !r.IsSuccess)
            })
            .OrderBy(x => x.Date)
            .ToList();
        
        // 热门语言统计
        var languageStats = records
            .Where(r => !string.IsNullOrEmpty(r.Language))
            .GroupBy(r => r.Language)
            .Select(g => new
            {
                Language = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();
        
        // 热门音色统计
        var voiceStats = records
            .Where(r => !string.IsNullOrEmpty(r.Voice))
            .GroupBy(r => r.Voice)
            .Select(g => new
            {
                Voice = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();
        
        // IP访问频率统计
        var ipStats = records
            .Where(r => !string.IsNullOrEmpty(r.IpAddress))
            .GroupBy(r => r.IpAddress)
            .Select(g => new
            {
                IpAddress = g.Key,
                Count = g.Count(),
                LastAccess = g.Max(r => r.CreatedAt)
            })
            .OrderByDescending(x => x.Count)
            .Take(20)
            .ToList();
        
        // 总体统计
        var totalRecords = await _context.TtsConversionRecords.CountAsync();
        var todayRecords = await _context.TtsConversionRecords
            .CountAsync(r => r.CreatedAt.Date == DateTime.Today);
        var successRate = totalRecords > 0 
            ? (await _context.TtsConversionRecords.CountAsync(r => r.IsSuccess) * 100.0 / totalRecords) 
            : 0;
        
        ViewBag.DailyStats = dailyStats;
        ViewBag.LanguageStats = languageStats;
        ViewBag.VoiceStats = voiceStats;
        ViewBag.IpStats = ipStats;
        ViewBag.TotalRecords = totalRecords;
        ViewBag.TodayRecords = todayRecords;
        ViewBag.SuccessRate = successRate;
        
        return View();
    }
}
