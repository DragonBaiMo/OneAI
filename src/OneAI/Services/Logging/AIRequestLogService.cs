using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using OneAI.Entities;
using OneAI.Services.AI.Models.Responses.Input;

namespace OneAI.Services.Logging;

/// <summary>
/// AI请求日志服务（生产者）- 将日志操作写入 Channel，不阻塞主业务
/// </summary>
public class AIRequestLogService
{
    private readonly Channel<LogQueueItem> _logChannel;
    private readonly ILogger<AIRequestLogService> _logger;
    private long _logIdCounter = 0; // 临时ID生成器（用于跟踪，真实ID由数据库生成）

    public AIRequestLogService(
        Channel<LogQueueItem> logChannel,
        ILogger<AIRequestLogService> logger)
    {
        _logChannel = logChannel;
        _logger = logger;
    }

    /// <summary>
    /// 创建新的请求日志（在请求开始时调用）- 异步写入 Channel
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="request">请求输入</param>
    /// <param name="account">使用的AI账户</param>
    /// <param name="sessionStickinessUsed">是否使用了会话粘性</param>
    /// <returns>临时日志ID和Stopwatch用于性能测量</returns>
    public async Task<(long TempLogId, Stopwatch Stopwatch)> CreateRequestLog(
        HttpContext context,
        ResponsesInput request,
        AIAccount? account,
        bool sessionStickinessUsed = false)
    {
        var stopwatch = Stopwatch.StartNew();
        var now = DateTime.UtcNow;

        // 生成临时ID（用于后续更新）
        var tempLogId = Interlocked.Increment(ref _logIdCounter);

        // 提取请求头信息
        var conversationId = context.Request.Headers.TryGetValue("conversation_id", out var convId)
            ? convId.ToString()
            : null;

        var sessionId = context.Request.Headers.TryGetValue("session_id", out var sessId)
            ? sessId.ToString()
            : null;

        var originator = context.Request.Headers.TryGetValue("originator", out var orig)
            ? orig.ToString()
            : "unknown";

        // 序列化请求参数（排除大字段）
        var requestParams = SerializeRequestParams(request);

        var log = new AIRequestLog
        {
            RequestId = Guid.NewGuid().ToString("N"),
            ConversationId = conversationId,
            SessionId = sessionId,

            // 请求信息
            AccountId = account?.Id,
            Provider = account?.Provider,
            Model = request.Model,
            Instructions = TruncateString(request.Instructions, 2000),
            IsStreaming = request.Stream ?? false,
            RequestParams = requestParams,
            MessageSummary = string.Empty,

            // 初始状态
            IsSuccess = false,
            RetryCount = 0,
            TotalAttempts = 1,
            SessionStickinessUsed = sessionStickinessUsed,

            // 时间信息
            RequestStartTime = now,
            CreatedAt = now,
            UpdatedAt = now,

            // 客户端信息
            ClientIp = GetClientIp(context),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Originator = originator
        };

        // 写入 Channel（非阻塞）
        var queueItem = new LogQueueItem
        {
            OperationType = LogOperationType.Create,
            Log = log,
            UpdateData = new Dictionary<string, object?>
            {
                { "TempLogId", tempLogId } // 临时ID用于映射
            }
        };

        try
        {
            await _logChannel.Writer.WriteAsync(queueItem);
            _logger.LogDebug(
                "日志入队 [TempLogId={TempLogId}, RequestId={RequestId}, Model={Model}]",
                tempLogId, log.RequestId, log.Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "日志入队失败 [TempLogId={TempLogId}]", tempLogId);
            // 即使日志失败，也不影响主业务流程
        }

        return (tempLogId, stopwatch);
    }

    /// <summary>
    /// 更新日志 - 记录重试（异步写入 Channel）
    /// </summary>
    public async Task UpdateRetry(long tempLogId, int attemptNumber, int? newAccountId = null)
    {
        var queueItem = new LogQueueItem
        {
            OperationType = LogOperationType.UpdateRetry,
            LogId = tempLogId, // 使用临时ID
            UpdateData = new Dictionary<string, object?>
            {
                { "RetryCount", attemptNumber - 1 },
                { "TotalAttempts", attemptNumber },
                { "AccountId", newAccountId },
                { "UpdatedAt", DateTime.UtcNow }
            }
        };

        try
        {
            await _logChannel.Writer.WriteAsync(queueItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重试日志入队失败 [TempLogId={TempLogId}]", tempLogId);
        }
    }

    /// <summary>
    /// 记录成功响应（异步写入 Channel）
    /// </summary>
    public async Task RecordSuccess(
        long tempLogId,
        Stopwatch stopwatch,
        int statusCode,
        long? timeToFirstByteMs = null,
        string? quotaInfo = null,
        int? promptTokens = null,
        int? completionTokens = null,
        int? totalTokens = null)
    {
        stopwatch.Stop();
        var now = DateTime.Now;

        var queueItem = new LogQueueItem
        {
            OperationType = LogOperationType.RecordSuccess,
            LogId = tempLogId,
            UpdateData = new Dictionary<string, object?>
            {
                { "IsSuccess", true },
                { "StatusCode", statusCode },
                { "RequestEndTime", now },
                { "DurationMs", stopwatch.ElapsedMilliseconds },
                { "TimeToFirstByteMs", timeToFirstByteMs },
                { "QuotaInfo", quotaInfo },
                { "PromptTokens", promptTokens },
                { "CompletionTokens", completionTokens },
                { "TotalTokens", totalTokens },
                { "UpdatedAt", now }
            }
        };

        try
        {
            await _logChannel.Writer.WriteAsync(queueItem);
            _logger.LogDebug(
                "成功日志入队 [TempLogId={TempLogId}, Duration={Duration}ms, Tokens={Tokens}]",
                tempLogId, stopwatch.ElapsedMilliseconds, totalTokens ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "成功日志入队失败 [TempLogId={TempLogId}]", tempLogId);
        }
    }

    /// <summary>
    /// 记录失败响应（异步写入 Channel）
    /// </summary>
    public async Task RecordFailure(
        long tempLogId,
        Stopwatch stopwatch,
        int? statusCode,
        string errorMessage,
        bool isRateLimited = false,
        int? rateLimitResetSeconds = null,
        string? quotaInfo = null)
    {
        stopwatch.Stop();
        var now = DateTime.Now;

        var queueItem = new LogQueueItem
        {
            OperationType = LogOperationType.RecordFailure,
            LogId = tempLogId,
            UpdateData = new Dictionary<string, object?>
            {
                { "IsSuccess", false },
                { "StatusCode", statusCode },
                { "ErrorMessage", TruncateString(errorMessage, 5000) },
                { "RequestEndTime", now },
                { "DurationMs", stopwatch.ElapsedMilliseconds },
                { "IsRateLimited", isRateLimited },
                { "RateLimitResetSeconds", rateLimitResetSeconds },
                { "QuotaInfo", quotaInfo },
                { "UpdatedAt", now }
            }
        };

        try
        {
            await _logChannel.Writer.WriteAsync(queueItem);
            _logger.LogDebug(
                "失败日志入队 [TempLogId={TempLogId}, StatusCode={StatusCode}, Duration={Duration}ms]",
                tempLogId, statusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "失败日志入队失败 [TempLogId={TempLogId}]", tempLogId);
        }
    }

    /// <summary>
    /// 序列化请求参数（排除大字段）
    /// </summary>
    private string? SerializeRequestParams(ResponsesInput request)
    {
        try
        {
            var paramsObj = new
            {
                model = request.Model,
                stream = request.Stream,
                temperature = request.Temperature,
                top_p = request.TopP,
                max_output_tokens = request.MaxOutputTokens,
                store = request.Store,
                service_tier = request.ServiceTier,
                message_count = request.Inputs?.Count ?? 0,
                tools_count = request.Tools?.Count ?? 0
            };

            return JsonSerializer.Serialize(paramsObj);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取客户端真实IP
    /// </summary>
    private string? GetClientIp(HttpContext context)
    {
        // 尝试从各种头部获取真实IP
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString();

        // X-Forwarded-For 可能包含多个IP，取第一个
        if (!string.IsNullOrEmpty(ip) && ip.Contains(','))
        {
            ip = ip.Split(',')[0].Trim();
        }

        return ip;
    }

    /// <summary>
    /// 截断字符串
    /// </summary>
    private string? TruncateString(string? input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength) + "...";
    }

}
