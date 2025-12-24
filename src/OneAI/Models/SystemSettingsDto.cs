namespace OneAI.Models;

/// <summary>
/// 系统设置 DTO
/// </summary>
public class SystemSettingsDto
{
    /// <summary>
    /// 设置键
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// 设置值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 设置描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 系统设置更新请求
/// </summary>
public class UpdateSystemSettingRequest
{
    /// <summary>
    /// 设置值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 设置描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// API Key 信息响应
/// </summary>
public class ApiKeyInfoResponse
{
    /// <summary>
    /// 是否存在 API Key
    /// </summary>
    public bool HasApiKey { get; set; }

    /// <summary>
    /// API Key（脱敏显示）
    /// </summary>
    public string? MaskedApiKey { get; set; }

    /// <summary>
    /// API Key 长度
    /// </summary>
    public int? ApiKeyLength { get; set; }
}

/// <summary>
/// Token 刷新信息响应
/// </summary>
public class TokenRefreshInfoResponse
{
    /// <summary>
    /// 当前 Token 过期时间（UTC）
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// 距离过期的剩余秒数
    /// </summary>
    public int? SecondsUntilExpiry { get; set; }

    /// <summary>
    /// 是否需要刷新
    /// </summary>
    public bool NeedsRefresh { get; set; }

    /// <summary>
    /// 刷新前剩余时间（分钟）
    /// </summary>
    public int RefreshBeforeExpiryMinutes { get; set; } = 5;
}
