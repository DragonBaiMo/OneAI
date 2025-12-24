using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OneAI.Data;
using OneAI.Entities;

namespace OneAI.Services;

/// <summary>
/// 系统设置服务实现
/// </summary>
public class SettingsService(AppDbContext dbContext) : ISettingsService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly Dictionary<string, string?> _settingsCache = new();
    private bool _initialized = false;

    /// <summary>
    /// 初始化设置（加载到内存缓存）
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await RefreshCacheAsync();
    }

    /// <summary>
    /// 清空缓存并重新加载
    /// </summary>
    public async Task RefreshCacheAsync()
    {
        _settingsCache.Clear();

        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .ToListAsync();

        foreach (var setting in settings)
        {
            _settingsCache[setting.Key] = setting.Value;
        }

        _initialized = true;
    }

    /// <summary>
    /// 获取设置值
    /// </summary>
    public async Task<string?> GetSettingAsync(string key)
    {
        await EnsureInitializedAsync();

        if (_settingsCache.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// 获取设置值，带类型转换
    /// </summary>
    public async Task<T?> GetSettingAsync<T>(string key) where T : class
    {
        var value = await GetSettingAsync(key);

        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(value, JsonSerializerOptions.Web);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 设置值（持久化到数据库）
    /// </summary>
    public async Task SetSettingAsync(string key, string? value, string? description = null)
    {
        await EnsureInitializedAsync();

        var setting = await _dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new SystemSettings
            {
                Key = key,
                Value = value,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            _dbContext.SystemSettings.Update(setting);
        }

        await _dbContext.SaveChangesAsync();

        // 更新缓存
        _settingsCache[key] = value;
    }

    /// <summary>
    /// 批量获取所有设置
    /// </summary>
    public async Task<Dictionary<string, string?>> GetAllSettingsAsync()
    {
        await EnsureInitializedAsync();

        return new Dictionary<string, string?>(_settingsCache);
    }

    /// <summary>
    /// 获取设置实体
    /// </summary>
    public async Task<SystemSettings?> GetSettingEntityAsync(string key)
    {
        return await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key);
    }

    /// <summary>
    /// 删除设置
    /// </summary>
    public async Task DeleteSettingAsync(string key)
    {
        var setting = await _dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting != null)
        {
            _dbContext.SystemSettings.Remove(setting);
            await _dbContext.SaveChangesAsync();

            _settingsCache.Remove(key);
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }
    }
}
