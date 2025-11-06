using System.ComponentModel.DataAnnotations;

namespace TtsWebApp.Models;

// 管理员用户
public class AdminUser
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? LastLoginAt { get; set; }
}

// 网站配置
public class SiteConfig
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// 文章类型枚举
public enum ArticleType
{
    Article = 0,      // 普通文章
    Page = 1          // 页面（如关于我们、隐私政策等）
}

// 文章
public class Article
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Summary { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Author { get; set; }
    
    [StringLength(200)]
    public string? Tags { get; set; }
    
    public bool IsPublished { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? PublishedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public int ViewCount { get; set; } = 0;
    
    // 文章类型：0=文章，1=页面
    public ArticleType Type { get; set; } = ArticleType.Article;
    
    // 页面标识符（用于页面类型，如 "about", "privacy" 等）
    [StringLength(50)]
    public string? Slug { get; set; }
}
