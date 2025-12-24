namespace OneAI.Entities;

/// <summary>
/// 系统设置实体
/// </summary>
public class SystemSettings
{
    /// <summary>
    /// 设置 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 设置键（唯一标识）
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
    /// 设置数据类型（string, int, bool, json等）
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
