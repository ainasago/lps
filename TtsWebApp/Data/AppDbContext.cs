using Microsoft.EntityFrameworkCore;
using TtsWebApp.Models;

namespace TtsWebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<SiteConfig> SiteConfigs { get; set; }
    public DbSet<Article> Articles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 配置唯一索引
        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        modelBuilder.Entity<SiteConfig>()
            .HasIndex(c => c.Key)
            .IsUnique();
        
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Slug)
            .IsUnique();
        
        // 初始化默认配置
        modelBuilder.Entity<SiteConfig>().HasData(
            new SiteConfig { Id = 1, Key = "SiteName", Value = "安妮语音转换", Description = "网站名称（中文）" },
            new SiteConfig { Id = 2, Key = "SiteNameEn", Value = "Annie TTS", Description = "网站名称（英文）" },
            new SiteConfig { Id = 3, Key = "ContactEmail", Value = "contact@annietts.com", Description = "联系邮箱" },
            new SiteConfig { Id = 4, Key = "SiteDescription", Value = "免费在线AI文字转语音工具，支持多种语言和自然人声", Description = "网站描述" },
            new SiteConfig { Id = 5, Key = "SiteUrl", Value = "https://www.annietts.com", Description = "网站URL" }
        );
        
        // 初始化默认管理员账号 (用户名: admin, 密码: admin123)
        // 密码使用 BCrypt 哈希
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        modelBuilder.Entity<AdminUser>().HasData(
            new AdminUser 
            { 
                Id = 1, 
                Username = "admin", 
                PasswordHash = passwordHash,
                CreatedAt = new DateTime(2025, 1, 1)
            }
        );
        
        // 页面数据在 Program.cs 中手动初始化
    }
}
