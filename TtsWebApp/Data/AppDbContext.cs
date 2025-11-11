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
    public DbSet<TtsConversionRecord> TtsConversionRecords { get; set; }
    
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
    }
}
