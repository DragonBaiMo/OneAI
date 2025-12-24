using OneAI.Entities;

namespace OneAI.Services.Logging;

/// <summary>
/// 日志队列操作类型
/// </summary>
public enum LogOperationType
{
    Create,
    UpdateRetry,
    RecordSuccess,
    RecordFailure
}

/// <summary>
/// 日志队列项 - 用于在 Channel 中传递日志操作
/// </summary>
public class LogQueueItem
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public LogOperationType OperationType { get; set; }

    /// <summary>
    /// 日志ID（用于更新操作）
    /// </summary>
    public long? LogId { get; set; }

    /// <summary>
    /// 完整的日志实体（用于创建操作）
    /// </summary>
    public AIRequestLog? Log { get; set; }

    /// <summary>
    /// 更新数据（用于更新操作）
    /// </summary>
    public Dictionary<string, object?>? UpdateData { get; set; }

    /// <summary>
    /// 创建时间戳（用于监控队列延迟）
    /// </summary>
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
}
