using Microsoft.EntityFrameworkCore;
using OneAI.Data;
using OneAI.Entities;

namespace OneAI.Services.Logging;

/// <summary>
/// AI请求数据聚合后台服务 - 定期将原始日志聚合为小时级别的统计数据
/// </summary>
public class AIRequestAggregationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AIRequestAggregationBackgroundService> _logger;

    // 检查间隔：每10分钟检查一次
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10);

    // 聚合延迟：当前小时完成后延迟5分钟再聚合（等待所有请求记录完毕）
    private readonly TimeSpan _aggregationDelay = TimeSpan.FromMinutes(5);

    public AIRequestAggregationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AIRequestAggregationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI请求数据聚合后台服务已启动");

        // 启动后延迟2分钟再开始（等待应用完全启动）
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        // 首次启动时，聚合历史数据（只执行一次）
        await InitializeHistoricalDataAsync(stoppingToken);

        // 进入定期聚合循环
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHourlyAggregationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行小时聚合时发生错误");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("AI请求数据聚合后台服务已停止");
    }

    /// <summary>
    /// 初始化历史数据聚合（仅在首次启动时执行）
    /// </summary>
    private async Task InitializeHistoricalDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();

        try
        {
            // 检查是否已有聚合数据
            var hasExistingData = await logDbContext.Set<AIRequestHourlySummary>()
                .AnyAsync(cancellationToken);

            if (hasExistingData)
            {
                _logger.LogInformation("检测到已有聚合数据，跳过历史数据初始化");
                return;
            }

            // 查找最早的日志记录时间
            var earliestLog = await logDbContext.AIRequestLogs
                .Where(log => log.RequestEndTime != null) // 仅处理已完成的请求
                .OrderBy(log => log.RequestStartTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (earliestLog == null)
            {
                _logger.LogInformation("没有找到历史日志数据，无需初始化");
                return;
            }

            // 计算需要聚合的时间范围
            var startHour = new DateTime(
                earliestLog.RequestStartTime.Year,
                earliestLog.RequestStartTime.Month,
                earliestLog.RequestStartTime.Day,
                earliestLog.RequestStartTime.Hour,
                0, 0, DateTimeKind.Utc);

            var currentHour = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour);
            var aggregationEndHour = currentHour.AddHours(-1); // 不聚合当前小时

            _logger.LogInformation(
                "开始初始化历史数据聚合 [起始时间: {StartHour}, 结束时间: {EndHour}]",
                startHour, aggregationEndHour);

            var aggregatedCount = 0;
            var currentAggregationHour = startHour;

            // 逐小时聚合
            while (currentAggregationHour <= aggregationEndHour && !cancellationToken.IsCancellationRequested)
            {
                await AggregateHourAsync(currentAggregationHour, logDbContext, cancellationToken);
                aggregatedCount++;
                currentAggregationHour = currentAggregationHour.AddHours(1);

                // 每聚合10小时，输出进度日志
                if (aggregatedCount % 10 == 0)
                {
                    _logger.LogInformation("历史数据聚合进度: 已完成 {Count} 小时", aggregatedCount);
                }
            }

            _logger.LogInformation("历史数据聚合完成，共处理 {Count} 小时的数据", aggregatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化历史数据聚合失败");
            // 不抛出异常，允许服务继续运行
        }
    }

    /// <summary>
    /// 执行小时级别的定期聚合
    /// </summary>
    private async Task PerformHourlyAggregationAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var logDbContext = scope.ServiceProvider.GetRequiredService<LogDbContext>();

        var now = DateTime.UtcNow;

        // 计算需要聚合的小时（当前时间 - 聚合延迟 - 1小时）
        // 例如：现在是 15:12，延迟5分钟，则聚合 14:00-15:00 这一小时
        var hourToAggregate = now
            .Subtract(_aggregationDelay)
            .Date
            .AddHours(now.Subtract(_aggregationDelay).Hour);

        // 检查该小时是否已聚合
        var alreadyAggregated = await logDbContext.Set<AIRequestHourlySummary>()
            .AnyAsync(s => s.HourStartTime == hourToAggregate, cancellationToken);

        if (alreadyAggregated)
        {
            _logger.LogDebug("小时 {Hour} 已聚合，跳过", hourToAggregate);
            return;
        }

        // 检查是否有该小时的日志数据
        var hourEndTime = hourToAggregate.AddHours(1);
        var hasLogs = await logDbContext.AIRequestLogs
            .AnyAsync(log =>
                log.RequestStartTime >= hourToAggregate &&
                log.RequestStartTime < hourEndTime &&
                log.RequestEndTime != null, // 仅统计已完成的请求
                cancellationToken);

        if (!hasLogs)
        {
            _logger.LogDebug("小时 {Hour} 没有日志数据，跳过聚合", hourToAggregate);
            return;
        }

        _logger.LogInformation("开始聚合小时 {Hour} 的数据", hourToAggregate);

        try
        {
            await AggregateHourAsync(hourToAggregate, logDbContext, cancellationToken);
            _logger.LogInformation("成功聚合小时 {Hour} 的数据", hourToAggregate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "聚合小时 {Hour} 的数据失败", hourToAggregate);
        }
    }

    /// <summary>
    /// 聚合指定小时的数据（核心方法）
    /// </summary>
    private async Task AggregateHourAsync(
        DateTime hourStartTime,
        LogDbContext logDbContext,
        CancellationToken cancellationToken)
    {
        var hourEndTime = hourStartTime.AddHours(1);

        // 获取该小时内所有已完成的请求
        var logsInHour = await logDbContext.AIRequestLogs
            .Where(log =>
                log.RequestStartTime >= hourStartTime &&
                log.RequestStartTime < hourEndTime &&
                log.RequestEndTime != null) // 仅统计已完成的请求
            .ToListAsync(cancellationToken);

        if (logsInHour.Count == 0)
        {
            _logger.LogDebug("小时 {Hour} 没有已完成的请求，跳过", hourStartTime);
            return;
        }

        var now = DateTime.UtcNow;

        // 1. 聚合总体统计
        await AggregateSummaryAsync(hourStartTime, logsInHour, now, logDbContext, cancellationToken);

        // 2. 聚合按模型统计
        await AggregateByModelAsync(hourStartTime, logsInHour, now, logDbContext, cancellationToken);

        // 3. 聚合按账户统计
        await AggregateByAccountAsync(hourStartTime, logsInHour, now, logDbContext, cancellationToken);
    }

    /// <summary>
    /// 聚合总体统计数据
    /// </summary>
    private async Task AggregateSummaryAsync(
        DateTime hourStartTime,
        List<AIRequestLog> logs,
        DateTime timestamp,
        LogDbContext logDbContext,
        CancellationToken cancellationToken)
    {
        var totalRequests = logs.Count;
        var successRequests = logs.Count(l => l.IsSuccess);
        var failedRequests = totalRequests - successRequests;
        var successRate = totalRequests > 0 ? (double)successRequests / totalRequests : 0;

        var durations = logs.Where(l => l.DurationMs.HasValue)
            .Select(l => l.DurationMs!.Value)
            .OrderBy(d => d)
            .ToList();

        var summary = new AIRequestHourlySummary
        {
            HourStartTime = hourStartTime,
            TotalRequests = totalRequests,
            SuccessRequests = successRequests,
            FailedRequests = failedRequests,
            SuccessRate = successRate,
            StreamingRequests = logs.Count(l => l.IsStreaming),
            TotalRetries = logs.Sum(l => l.RetryCount),
            RateLimitedRequests = logs.Count(l => l.IsRateLimited),

            TotalPromptTokens = logs.Sum(l => l.PromptTokens ?? 0),
            TotalCompletionTokens = logs.Sum(l => l.CompletionTokens ?? 0),
            TotalTokens = logs.Sum(l => l.TotalTokens ?? 0),
            AvgTokensPerRequest = totalRequests > 0
                ? logs.Average(l => l.TotalTokens ?? 0)
                : 0,

            AvgDurationMs = durations.Count > 0 ? durations.Average() : 0,
            MinDurationMs = durations.FirstOrDefault(),
            MaxDurationMs = durations.LastOrDefault(),
            P50DurationMs = CalculatePercentile(durations, 0.5),
            P95DurationMs = CalculatePercentile(durations, 0.95),
            P99DurationMs = CalculatePercentile(durations, 0.99),
            AvgTimeToFirstByteMs = logs
                .Where(l => l.TimeToFirstByteMs.HasValue)
                .Select(l => (double)l.TimeToFirstByteMs!.Value)
                .DefaultIfEmpty(0)
                .Average(),

            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Version = 1
        };

        logDbContext.Set<AIRequestHourlySummary>().Add(summary);
        await logDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 聚合按模型维度的统计数据
    /// </summary>
    private async Task AggregateByModelAsync(
        DateTime hourStartTime,
        List<AIRequestLog> logs,
        DateTime timestamp,
        LogDbContext logDbContext,
        CancellationToken cancellationToken)
    {
        var groupedByModel = logs.GroupBy(l => new { l.Model, l.Provider });

        foreach (var group in groupedByModel)
        {
            var modelLogs = group.ToList();
            var totalRequests = modelLogs.Count;
            var successRequests = modelLogs.Count(l => l.IsSuccess);

            var durations = modelLogs
                .Where(l => l.DurationMs.HasValue)
                .Select(l => l.DurationMs!.Value)
                .ToList();

            var modelSummary = new AIRequestHourlyByModel
            {
                HourStartTime = hourStartTime,
                Model = group.Key.Model,
                Provider = group.Key.Provider,
                TotalRequests = totalRequests,
                SuccessRequests = successRequests,
                FailedRequests = totalRequests - successRequests,
                SuccessRate = totalRequests > 0 ? (double)successRequests / totalRequests : 0,
                StreamingRequests = modelLogs.Count(l => l.IsStreaming),
                TotalRetries = modelLogs.Sum(l => l.RetryCount),

                TotalPromptTokens = modelLogs.Sum(l => l.PromptTokens ?? 0),
                TotalCompletionTokens = modelLogs.Sum(l => l.CompletionTokens ?? 0),
                TotalTokens = modelLogs.Sum(l => l.TotalTokens ?? 0),
                AvgTokensPerRequest = totalRequests > 0
                    ? modelLogs.Average(l => l.TotalTokens ?? 0)
                    : 0,

                AvgDurationMs = durations.Count > 0 ? durations.Average() : 0,
                MinDurationMs = durations.DefaultIfEmpty(0).Min(),
                MaxDurationMs = durations.DefaultIfEmpty(0).Max(),
                AvgTimeToFirstByteMs = modelLogs
                    .Where(l => l.TimeToFirstByteMs.HasValue)
                    .Select(l => (double)l.TimeToFirstByteMs!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),

                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                Version = 1
            };

            logDbContext.Set<AIRequestHourlyByModel>().Add(modelSummary);
        }

        await logDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 聚合按账户维度的统计数据
    /// </summary>
    private async Task AggregateByAccountAsync(
        DateTime hourStartTime,
        List<AIRequestLog> logs,
        DateTime timestamp,
        LogDbContext logDbContext,
        CancellationToken cancellationToken)
    {
        // 获取有账户ID的日志
        var logsWithAccount = logs.Where(l => l.AccountId.HasValue).ToList();

        if (logsWithAccount.Count == 0)
        {
            return;
        }

        // 从主数据库获取账户信息
        using var appScope = _serviceProvider.CreateScope();
        var appDbContext = appScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var accountIds = logsWithAccount.Select(l => l.AccountId!.Value).Distinct().ToList();
        var accounts = await appDbContext.AIAccounts
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var groupedByAccount = logsWithAccount.GroupBy(l => l.AccountId!.Value);

        foreach (var group in groupedByAccount)
        {
            var accountLogs = group.ToList();
            var totalRequests = accountLogs.Count;
            var successRequests = accountLogs.Count(l => l.IsSuccess);

            var durations = accountLogs
                .Where(l => l.DurationMs.HasValue)
                .Select(l => l.DurationMs!.Value)
                .ToList();

            var account = accounts.GetValueOrDefault(group.Key);

            var accountSummary = new AIRequestHourlyByAccount
            {
                HourStartTime = hourStartTime,
                AccountId = group.Key,
                AccountName = account?.Name,
                Provider = account?.Provider,
                TotalRequests = totalRequests,
                SuccessRequests = successRequests,
                FailedRequests = totalRequests - successRequests,
                SuccessRate = totalRequests > 0 ? (double)successRequests / totalRequests : 0,
                RateLimitedRequests = accountLogs.Count(l => l.IsRateLimited),
                TotalRetries = accountLogs.Sum(l => l.RetryCount),

                TotalPromptTokens = accountLogs.Sum(l => l.PromptTokens ?? 0),
                TotalCompletionTokens = accountLogs.Sum(l => l.CompletionTokens ?? 0),
                TotalTokens = accountLogs.Sum(l => l.TotalTokens ?? 0),
                AvgTokensPerRequest = totalRequests > 0
                    ? accountLogs.Average(l => l.TotalTokens ?? 0)
                    : 0,

                AvgDurationMs = durations.Count > 0 ? durations.Average() : 0,
                MinDurationMs = durations.DefaultIfEmpty(0).Min(),
                MaxDurationMs = durations.DefaultIfEmpty(0).Max(),
                AvgTimeToFirstByteMs = accountLogs
                    .Where(l => l.TimeToFirstByteMs.HasValue)
                    .Select(l => (double)l.TimeToFirstByteMs!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),

                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                Version = 1
            };

            logDbContext.Set<AIRequestHourlyByAccount>().Add(accountSummary);
        }

        await logDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 计算百分位数
    /// </summary>
    private long? CalculatePercentile(List<long> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return null;

        var index = (int)Math.Ceiling(sortedValues.Count * percentile) - 1;
        index = Math.Max(0, Math.Min(index, sortedValues.Count - 1));

        return sortedValues[index];
    }
}
