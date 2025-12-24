using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneAI.Data;
using OneAI.Models;

namespace OneAI.Endpoints;

public static class AIRequestLogEndpoints
{
    public static void MapAIRequestLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs")
            .WithTags("日志管理")
            .RequireAuthorization();

        // 查询日志列表
        group.MapPost("/query", QueryLogs)
            .WithName("QueryAIRequestLogs")
            .WithDescription("查询AI请求日志");

        // 获取日志统计信息
        group.MapGet("/statistics", GetStatistics)
            .WithName("GetLogStatistics")
            .WithDescription("获取日志统计信息");
    }

    private static async Task<ApiResponse<PagedResponse<AIRequestLogDto>>> QueryLogs(
        [FromBody] AIRequestLogQueryRequest request,
        LogDbContext logDbContext,
        AppDbContext appDbContext)
    {
        try
        {
                // 设置默认时间范围（最近7天）
                var endTime = request.EndTime ?? DateTime.UtcNow;
                var startTime = request.StartTime ?? endTime.AddDays(-7);

                // 构建查询
                var query = logDbContext.AIRequestLogs
                    .Where(log => log.RequestStartTime >= startTime && log.RequestStartTime <= endTime);

                // 应用过滤条件
                if (request.AccountId.HasValue)
                {
                    query = query.Where(log => log.AccountId == request.AccountId.Value);
                }

                if (!string.IsNullOrEmpty(request.Model))
                {
                    query = query.Where(log => log.Model == request.Model);
                }

                if (request.IsSuccess.HasValue)
                {
                    query = query.Where(log => log.IsSuccess == request.IsSuccess.Value);
                }

                // 获取总数
                var totalCount = await query.CountAsync();

                // 应用分页和排序
                var logs = await query
                    .OrderByDescending(log => log.RequestStartTime)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                // 获取所有相关的账户ID
                var accountIds = logs
                    .Where(log => log.AccountId.HasValue)
                    .Select(log => log.AccountId!.Value)
                    .Distinct()
                    .ToList();

                // 从主数据库获取账户信息
                var accounts = await appDbContext.AIAccounts
                    .Where(a => accountIds.Contains(a.Id))
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Email,
                        a.Provider
                    })
                    .ToDictionaryAsync(a => a.Id);

                // 组装响应数据
                var logDtos = logs.Select(log =>
                {
                    var account = log.AccountId.HasValue && accounts.ContainsKey(log.AccountId.Value)
                        ? accounts[log.AccountId.Value]
                        : null;

                    return new AIRequestLogDto
                    {
                        Id = log.Id,
                        RequestId = log.RequestId,
                        ConversationId = log.ConversationId,
                        SessionId = log.SessionId,
                        AccountId = log.AccountId,
                        AccountName = account?.Name,
                        AccountEmail = account?.Email,
                        Provider = log.Provider ?? account?.Provider,
                        Model = log.Model,
                        IsStreaming = log.IsStreaming,
                        MessageSummary = log.MessageSummary,
                        StatusCode = log.StatusCode,
                        IsSuccess = log.IsSuccess,
                        ErrorMessage = log.ErrorMessage,
                        RetryCount = log.RetryCount,
                        TotalAttempts = log.TotalAttempts,
                        ResponseSummary = log.ResponseSummary,
                        PromptTokens = log.PromptTokens,
                        CompletionTokens = log.CompletionTokens,
                        TotalTokens = log.TotalTokens,
                        RequestStartTime = log.RequestStartTime,
                        RequestEndTime = log.RequestEndTime,
                        DurationMs = log.DurationMs,
                        TimeToFirstByteMs = log.TimeToFirstByteMs,
                        IsRateLimited = log.IsRateLimited,
                        RateLimitResetSeconds = log.RateLimitResetSeconds,
                        SessionStickinessUsed = log.SessionStickinessUsed,
                        ClientIp = log.ClientIp,
                        CreatedAt = log.CreatedAt
                    };
                }).ToList();

                var response = new PagedResponse<AIRequestLogDto>
                {
                    Items = logDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<AIRequestLogDto>>.Success(response, "查询成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<AIRequestLogDto>>.Fail($"查询日志失败: {ex.Message}", 500);
            }
        }

    private static async Task<ApiResponse<object>> GetStatistics(
        [FromQuery] int? accountId,
        [FromQuery] DateTime? startTime,
        [FromQuery] DateTime? endTime,
        LogDbContext logDbContext)
    {
        try
        {
                // 设置默认时间范围（最近7天）
                var end = endTime ?? DateTime.UtcNow;
                var start = startTime ?? end.AddDays(-7);

                // 构建查询
                var query = logDbContext.AIRequestLogs
                    .Where(log => log.RequestStartTime >= start && log.RequestStartTime <= end);

                if (accountId.HasValue)
                {
                    query = query.Where(log => log.AccountId == accountId.Value);
                }

                var totalRequests = await query.CountAsync();

                // 只统计已完成的请求（有结束时间）
                var completedQuery = query.Where(log => log.RequestEndTime != null);
                var completedRequests = await completedQuery.CountAsync();
                var successRequests = await completedQuery.CountAsync(log => log.IsSuccess);
                var failedRequests = completedRequests - successRequests;
                var inProgressRequests = totalRequests - completedRequests;

                var totalTokens = await completedQuery.SumAsync(log => log.TotalTokens ?? 0);
                var avgDurationMs = completedRequests > 0
                    ? await completedQuery.AverageAsync(log => log.DurationMs ?? 0)
                    : 0;

                // 按模型统计
                var modelStats = await query
                    .GroupBy(log => log.Model)
                    .Select(g => new
                    {
                        Model = g.Key,
                        Count = g.Count(),
                        TotalTokens = g.Sum(log => log.TotalTokens ?? 0)
                    })
                    .OrderByDescending(s => s.Count)
                    .Take(10)
                    .ToListAsync();

                var statistics = new
                {
                    totalRequests,
                    completedRequests,
                    successRequests,
                    failedRequests,
                    inProgressRequests,
                    successRate = completedRequests > 0 ? (double)successRequests / completedRequests : 0,
                    totalTokens,
                    avgDurationMs,
                    modelStats,
                    timeRange = new
                    {
                        start,
                        end
                    }
                };

                return ApiResponse<object>.Success(statistics, "获取统计信息成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"获取统计信息失败: {ex.Message}", 500);
            }
        }
}
