using OneAI.Entities;

namespace OneAI.Services;

/// <summary>
/// 系统设置服务接口
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 获取设置值
    /// </summary>
    Task<string?> GetSettingAsync(string key);

    /// <summary>
    /// 获取设置值，带类型转换
    /// </summary>
    Task<T?> GetSettingAsync<T>(string key) where T : class;

    /// <summary>
    /// 设置值（持久化到数据库）
    /// </summary>
    Task SetSettingAsync(string key, string? value, string? description = null);

    /// <summary>
    /// 批量获取所有设置
    /// </summary>
    Task<Dictionary<string, string?>> GetAllSettingsAsync();

    /// <summary>
    /// 获取设置实体
    /// </summary>
    Task<SystemSettings?> GetSettingEntityAsync(string key);

    /// <summary>
    /// 初始化设置（加载到内存缓存）
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 清空缓存并重新加载
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// 删除设置
    /// </summary>
    Task DeleteSettingAsync(string key);
}
