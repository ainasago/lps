using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace TtsWebApp.Models;

// 管理员用户
[Table(Name = "AdminUsers")]
[Index("idx_username", "Username", true)]
public class AdminUser
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    [Column(StringLength = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;
    
    [Column(IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Column(IsNullable = false, DbType = "TEXT")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [Column(IsNullable = true, DbType = "TEXT")]
    public DateTime? LastLoginAt { get; set; }
}

// 网站配置
[Table(Name = "SiteConfigs")]
[Index("idx_key", "Key", true)]
public class SiteConfig
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    [Column(StringLength = 100, IsNullable = false)]
    public string Key { get; set; } = string.Empty;
    
    [Column(IsNullable = false)]
    public string Value { get; set; } = string.Empty;
    
    [Column(StringLength = 200, IsNullable = true)]
    public string? Description { get; set; }
    
    [Column(IsNullable = false, DbType = "TEXT")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// 文章类型枚举
public enum ArticleType
{
    Article = 0,      // 普通文章
    Page = 1          // 页面（如关于我们、隐私政策等）
}

// 文章
[Table(Name = "Articles")]
[Index("idx_slug", "Slug", true)]
[Index("idx_type_published", "Type,IsPublished", false)]
public class Article
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    [Column(StringLength = 200, IsNullable = false)]
    public string Title { get; set; } = string.Empty;
    
    [Column(StringLength = 500, IsNullable = true)]
    public string? Summary { get; set; }
    
    [Column(StringLength = -1, IsNullable = false)]
    public string Content { get; set; } = string.Empty;
    
    [Column(StringLength = 100, IsNullable = true)]
    public string? Author { get; set; }
    
    [Column(StringLength = 200, IsNullable = true)]
    public string? Tags { get; set; }
    
    [Column(IsNullable = false)]
    public bool IsPublished { get; set; } = false;
    
    [Column(IsNullable = false, DbType = "TEXT")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [Column(IsNullable = true, DbType = "TEXT")]
    public DateTime? PublishedAt { get; set; }
    
    [Column(IsNullable = false, DbType = "TEXT")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Column(IsNullable = false)]
    public int ViewCount { get; set; } = 0;
    
    // 文章类型：0=文章，1=页面
    [Column(IsNullable = false)]
    public ArticleType Type { get; set; } = ArticleType.Article;
    
    // 页面标识符（用于页面类型，如 "about", "privacy" 等）
    [Column(StringLength = 50, IsNullable = true)]
    public string? Slug { get; set; }
    
    // 语言标识：zh-CN=中文, en-US=英文
    [Column(StringLength = 10, IsNullable = false)]
    public string Language { get; set; } = "zh-CN";
    
    // 关联文章ID（用于关联中英文版本，指向另一个语言的文章）
    [Column(IsNullable = true)]
    public int? RelatedArticleId { get; set; }
}

// TTS转换记录
[Table(Name = "TtsConversionRecords")]
[Index("idx_createdat", "CreatedAt DESC", false)]
[Index("idx_ip", "IpAddress", false)]
public class TtsConversionRecord
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    [Column(StringLength = -1, IsNullable = false)]
    public string Text { get; set; } = string.Empty;
    
    [Column(StringLength = 50, IsNullable = true)]
    public string? Language { get; set; }
    
    [Column(StringLength = 50, IsNullable = true)]
    public string? Voice { get; set; }
    
    [Column(IsNullable = true)]
    public int? Speed { get; set; }
    
    [Column(IsNullable = true)]
    public int? Pitch { get; set; }
    
    [Column(StringLength = 100, IsNullable = true)]
    public string? IpAddress { get; set; }
    
    [Column(StringLength = 500, IsNullable = true)]
    public string? UserAgent { get; set; }
    
    [Column(IsNullable = false, DbType = "TEXT")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // 转换是否成功
    [Column(IsNullable = false)]
    public bool IsSuccess { get; set; } = true;
    
    // 错误信息（如果失败）
    [Column(StringLength = -1, IsNullable = true)]
    public string? ErrorMessage { get; set; }
}
