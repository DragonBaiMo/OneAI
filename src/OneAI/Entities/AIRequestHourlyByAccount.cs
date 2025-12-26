namespace OneAI.Entities;

/// <summary>
/// AI请求每小时汇总统计 - 按账户维度
/// </summary>
public class AIRequestHourlyByAccount
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
    /// 账户ID
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// 账户名称（冗余字段，便于快速查询）
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// AI提供商
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
    /// 触发限流的请求数
    /// </summary>
    public int RateLimitedRequests { get; set; }

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
