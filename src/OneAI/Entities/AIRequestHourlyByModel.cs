namespace OneAI.Entities;

/// <summary>
/// AI请求每小时汇总统计 - 按模型维度
/// </summary>
public class AIRequestHourlyByModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 统计小时（UTC时间）
    /// </summary>
    public DateTime HourStartTime { get; set; }

    /// <summary>
    /// 模型名称（如：gpt-4, claude-3-opus等）
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// AI提供商（如：OpenAI, Claude, Gemini等）
    /// </summary>
    public string? Provider { get; set; }

    // ==================== 请求数量指标 ====================

    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 成功请求数
    /// </summary>
    public int SuccessRequests { get; set; }

    /// <summary>
    /// 失败请求数
    /// </summary>
    public int FailedRequests { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 流式请求数
    /// </summary>
    public int StreamingRequests { get; set; }

    /// <summary>
    /// 重试总次数
    /// </summary>
    public int TotalRetries { get; set; }

    // ==================== Token使用指标 ====================

    /// <summary>
    /// 总提示Token数
    /// </summary>
    public long TotalPromptTokens { get; set; }

    /// <summary>
    /// 总完成Token数
    /// </summary>
    public long TotalCompletionTokens { get; set; }

    /// <summary>
    /// 总Token数
    /// </summary>
    public long TotalTokens { get; set; }

    /// <summary>
    /// 平均每次请求Token数
    /// </summary>
    public double AvgTokensPerRequest { get; set; }

    // ==================== 性能指标 ====================

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AvgDurationMs { get; set; }

    /// <summary>
    /// 最小响应时间（毫秒）
    /// </summary>
    public long? MinDurationMs { get; set; }

    /// <summary>
    /// 最大响应时间（毫秒）
    /// </summary>
    public long? MaxDurationMs { get; set; }

    /// <summary>
    /// 平均首字节时间（毫秒）
    /// </summary>
    public double? AvgTimeToFirstByteMs { get; set; }

    // ==================== 元数据 ====================

    /// <summary>
    /// 聚合创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 聚合更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 数据版本号
    /// </summary>
    public int Version { get; set; } = 1;
}
