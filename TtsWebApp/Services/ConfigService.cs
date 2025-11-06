using Microsoft.EntityFrameworkCore;
using TtsWebApp.Data;
using TtsWebApp.Models;

namespace TtsWebApp.Services;

public interface IConfigService
{
    Task<string> GetConfigAsync(string key, string defaultValue = "");
    Task<Dictionary<string, string>> GetAllConfigsAsync();
    Task SetConfigAsync(string key, string value, string? description = null);
}

public class ConfigService : IConfigService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConfigService> _logger;
    
    public ConfigService(AppDbContext context, ILogger<ConfigService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<string> GetConfigAsync(string key, string defaultValue = "")
    {
        try
        {
            var config = await _context.SiteConfigs
                .FirstOrDefaultAsync(c => c.Key == key);
            
            return config?.Value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置失败: {Key}", key);
            return defaultValue;
        }
    }
    
    public async Task<Dictionary<string, string>> GetAllConfigsAsync()
    {
        try
        {
            var configs = await _context.SiteConfigs.ToListAsync();
            return configs.ToDictionary(c => c.Key, c => c.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有配置失败");
            return new Dictionary<string, string>();
        }
    }
    
    public async Task SetConfigAsync(string key, string value, string? description = null)
    {
        try
        {
            var config = await _context.SiteConfigs
                .FirstOrDefaultAsync(c => c.Key == key);
            
            if (config != null)
            {
                config.Value = value;
                config.UpdatedAt = DateTime.Now;
                if (description != null)
                {
                    config.Description = description;
                }
            }
            else
            {
                config = new SiteConfig
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    UpdatedAt = DateTime.Now
                };
                _context.SiteConfigs.Add(config);
            }
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置配置失败: {Key}", key);
            throw;
        }
    }
}
