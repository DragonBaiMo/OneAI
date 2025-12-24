using Microsoft.EntityFrameworkCore;
using OneAI.Constants;
using OneAI.Data;

namespace OneAI.Services.OpenAIOAuth;

/// <summary>
/// 后台服务：定期检查并刷新即将过期的 OAuth Token
/// </summary>
public class OAuthTokenRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OAuthTokenRefreshBackgroundService> _logger;

    // 检查间隔：每 5 分钟检查一次
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    // 提前刷新时间：当 token 还有 1 小时过期时就开始刷新
    private readonly TimeSpan _refreshThreshold = TimeSpan.FromHours(1);

    public OAuthTokenRefreshBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OAuthTokenRefreshBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OAuth Token 刷新后台服务已启动");

        // 启动后延迟 1 分钟再开始第一次检查
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRefreshTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查和刷新 OAuth Token 时发生错误");
            }

            // 等待下一次检查
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("OAuth Token 刷新后台服务已停止");
    }

    private async Task CheckAndRefreshTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var oAuthService = scope.ServiceProvider.GetRequiredService<OpenAIOAuthService>();

        // 获取所有启用的 OpenAI 账户
        var openAiAccounts = await dbContext.AIAccounts
            .Where(a => a.Provider == AIProviders.OpenAI && a.IsEnabled)
            .ToListAsync(cancellationToken);

        if (!openAiAccounts.Any())
        {
            _logger.LogDebug("没有找到需要检查的 OpenAI 账户");
            return;
        }

        _logger.LogInformation("开始检查 {Count} 个 OpenAI 账户的 OAuth Token", openAiAccounts.Count);

        var refreshedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;

        foreach (var account in openAiAccounts)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var oauthData = account.GetOpenAiOauth();

                // 如果没有 OAuth 数据或没有刷新令牌，跳过
                if (oauthData == null || string.IsNullOrEmpty(oauthData.RefreshToken))
                {
                    _logger.LogDebug(
                        "账户 {AccountId} ({Name}) 没有 OAuth 数据或刷新令牌，跳过",
                        account.Id,
                        account.Name ?? "未命名");
                    skippedCount++;
                    continue;
                }

                // 检查 token 是否即将过期
                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(oauthData.ExpiresAt).UtcDateTime;
                var now = DateTime.UtcNow;
                var timeUntilExpiry = expiresAt - now;

                _logger.LogDebug(
                    "账户 {AccountId} ({Name}) Token 将在 {Minutes} 分钟后过期 (过期时间: {ExpiresAt})",
                    account.Id,
                    account.Name ?? "未命名",
                    timeUntilExpiry.TotalMinutes,
                    expiresAt);

                // 如果 token 即将过期（或已过期），则刷新
                if (timeUntilExpiry <= _refreshThreshold)
                {
                    _logger.LogInformation(
                        "账户 {AccountId} ({Name}) 的 Token 即将过期，开始刷新... (剩余时间: {Minutes} 分钟)",
                        account.Id,
                        account.Name ?? "未命名",
                        timeUntilExpiry.TotalMinutes);

                    try
                    {
                        await oAuthService.RefreshOpenAiOAuthTokenAsync(account);

                        refreshedCount++;

                        _logger.LogInformation(
                            "成功刷新账户 {AccountId} ({Name}) 的 OAuth Token",
                            account.Id,
                            account.Name ?? "未命名");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;

                        _logger.LogError(
                            ex,
                            "刷新账户 {AccountId} ({Name}) 的 OAuth Token 失败",
                            account.Id,
                            account.Name ?? "未命名");
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "账户 {AccountId} ({Name}) Token 尚未到刷新阈值，跳过 (剩余: {Minutes} 分钟, 阈值: {Threshold} 分钟)",
                        account.Id,
                        account.Name ?? "未命名",
                        timeUntilExpiry.TotalMinutes,
                        _refreshThreshold.TotalMinutes);
                    skippedCount++;
                }
            }
            catch (Exception ex)
            {
                errorCount++;

                _logger.LogError(
                    ex,
                    "处理账户 {AccountId} ({Name}) 时发生错误",
                    account.Id,
                    account.Name ?? "未命名");
            }
        }

        _logger.LogInformation(
            "OAuth Token 检查完成 - 总数: {Total}, 刷新: {Refreshed}, 跳过: {Skipped}, 错误: {Errors}",
            openAiAccounts.Count,
            refreshedCount,
            skippedCount,
            errorCount);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止 OAuth Token 刷新后台服务...");
        await base.StopAsync(cancellationToken);
    }
}
