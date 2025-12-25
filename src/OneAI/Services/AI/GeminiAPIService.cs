using OneAI.Constants;
using OneAI.Data;
using OneAI.Entities;
using OneAI.Extensions;
using OneAI.Services.AI.Models.Gemini.Input;
using OneAI.Services.Logging;
using System.Net;
using System.Text.Json;
using OneAI.Services.AI.Gemini;

namespace OneAI.Services.AI;

/// <summary>
/// Gemini API 服务 - 处理 Gemini 原生 API 请求
/// </summary>
public class GeminiAPIService(
    AccountQuotaCacheService quotaCache,
    AIRequestLogService requestLogService,
    ILogger<GeminiAPIService> logger,
    IConfiguration configuration)
{
    // 静态HttpClient实例 - 避免套接字耗尽，提高性能
    private static readonly HttpClient HttpClient = new();

    private static readonly string[] ClientErrorKeywords =
    [
        "invalid_argument",
        "permission_denied",
        "resource_exhausted",
        "\"INVALID_ARGUMENT\""
    ];

    /// <summary>
    /// 执行 Gemini 内容生成请求（非流式）
    /// </summary>
    public async Task ExecuteGenerateContent(
        HttpContext context,
        GeminiInput input,
        string model,
        string? conversationId,
        AIAccountService aiAccountService)
    {
        // 重置AIProviderIds（每次尝试时清空）
        AIProviderAsyncLocal.AIProviderIds = new List<int>();
        const int maxRetries = 15;
        string? lastErrorMessage = null;
        HttpStatusCode? lastStatusCode = null;

        // 创建请求日志
        var (logId, stopwatch) = await requestLogService.CreateRequestLog(
            context,
            model,
            false,
            null,
            false);

        bool sessionStickinessUsed = false;

        try
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (context.Response.HasStarted)
                {
                    logger.LogWarning("响应已开始写入，无法继续重试");
                    await requestLogService.RecordFailure(
                        logId,
                        stopwatch,
                        context.Response.StatusCode,
                        "响应已开始写入，无法继续重试");
                    return;
                }

                logger.LogDebug("尝试第 {Attempt}/{MaxRetries} 次获取 Gemini 账户", attempt, maxRetries);

                // 会话粘性
                AIAccount? account = null;

                if (!string.IsNullOrEmpty(conversationId))
                {
                    var lastAccountId = quotaCache.GetConversationAccount(conversationId);
                    if (lastAccountId.HasValue)
                    {
                        account = await aiAccountService.TryGetAccountById(lastAccountId.Value);
                        if (account != null && account.Provider == AIProviders.Gemini)
                        {
                            sessionStickinessUsed = true;
                            logger.LogInformation(
                                "会话粘性成功：会话 {ConversationId} 复用 Gemini 账户 {AccountId}",
                                conversationId,
                                lastAccountId.Value);
                        }
                        else
                        {
                            account = null;
                        }
                    }
                }

                // 智能选择 Gemini 账户
                if (account == null)
                {
                    account = await aiAccountService.GetAIAccountByProvider(AIProviders.Gemini);
                }

                if (account == null)
                {
                    if (!string.IsNullOrEmpty(lastErrorMessage))
                    {
                        await context.Response.WriteAsJsonAsync(lastErrorMessage);
                        return;
                    }

                    lastErrorMessage = $"无可用的 Gemini 账户（尝试 {attempt}/{maxRetries}）";
                    lastStatusCode = HttpStatusCode.ServiceUnavailable;

                    logger.LogWarning(lastErrorMessage);

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000); // 等待后重试
                        continue;
                    }

                    break;
                }

                AIProviderAsyncLocal.AIProviderIds.Add(account.Id);

                var geminiOAuth = account.GetGeminiOauth();
                if (geminiOAuth == null)
                {
                    if (!string.IsNullOrEmpty(lastErrorMessage))
                    {
                        await context.Response.WriteAsJsonAsync(lastErrorMessage);
                        return;
                    }

                    lastErrorMessage = $"账户 {account.Id} 没有有效的 Gemini OAuth 凭证";
                    lastStatusCode = HttpStatusCode.Unauthorized;

                    logger.LogWarning(lastErrorMessage);

                    break;
                }

                // 发送请求
                try
                {
                    var codeAssistEndpoint = configuration["Gemini:CodeAssistEndpoint"] ??
                                             throw new InvalidOperationException(
                                                 "Gemini CodeAssistEndpoint is not configured");
                    var action = "generateContent";
                    var url = $"{codeAssistEndpoint}/v1internal:{action}";

                    var response = await SendGeminiRequest(
                        url,
                        input,
                        geminiOAuth.Token,
                        geminiOAuth.ProjectId,
                        model,
                        isStream: false);

                    await requestLogService.UpdateRetry(logId, attempt, account.Id);

                    // 处理响应
                    if (response.StatusCode >= HttpStatusCode.BadRequest)
                    {
                        var error = await response.Content.ReadAsStringAsync();

                        lastErrorMessage = error;
                        lastStatusCode = response.StatusCode;

                        logger.LogError(
                            "Gemini 请求失败 (账户: {AccountId}, 状态码: {StatusCode}, 尝试: {Attempt}/{MaxRetries}): {Error}",
                            account.Id,
                            response.StatusCode,
                            attempt,
                            maxRetries,
                            error);

                        // 检查是否是客户端错误
                        bool isClientError = ClientErrorKeywords.Any(keyword => error.Contains(keyword));

                        if (isClientError || response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await aiAccountService.DisableAccount(account.Id);

                            if (response.StatusCode == HttpStatusCode.Unauthorized || isClientError)
                            {
                                if (attempt < maxRetries)
                                {
                                    continue;
                                }

                                await requestLogService.RecordFailure(
                                    logId,
                                    stopwatch,
                                    (int)response.StatusCode,
                                    error);

                                break;
                            }
                        }

                        if (attempt < maxRetries)
                        {
                            continue;
                        }

                        await requestLogService.RecordFailure(
                            logId,
                            stopwatch,
                            (int)response.StatusCode,
                            error);

                        break;
                    }

                    // 成功响应
                    logger.LogInformation(
                        "成功处理 Gemini 请求 (账户: {AccountId}, 模型: {Model}, 尝试: {Attempt}/{MaxRetries})",
                        account.Id,
                        model,
                        attempt,maxRetries);

                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        quotaCache.SetConversationAccount(conversationId, account.Id);
                    }

                    await requestLogService.RecordSuccess(
                        logId,
                        stopwatch,
                        (int)response.StatusCode,
                        timeToFirstByteMs: stopwatch.ElapsedMilliseconds);

                    // 流式传输响应
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)response.StatusCode;

                    await response.Content.CopyToAsync(context.Response.Body);
                    return;
                }
                catch (Exception ex)
                {
                    lastErrorMessage = $"请求异常 (账户: {account.Id}): {ex.Message}";
                    lastStatusCode = HttpStatusCode.InternalServerError;

                    logger.LogError(ex, "Gemini 请求异常（尝试 {Attempt}/{MaxRetries}）", attempt, maxRetries);

                    if (attempt < maxRetries)
                    {
                        continue;
                    }

                    await requestLogService.RecordFailure(
                        logId,
                        stopwatch,
                        (int)lastStatusCode,
                        lastErrorMessage);

                    break;
                }
            }

            // 所有重试都失败
            context.Response.StatusCode = (int)(lastStatusCode ?? HttpStatusCode.ServiceUnavailable);

            var finalErrorMessage = lastErrorMessage ?? $"所有 {maxRetries} 次重试均失败，无法完成请求";

            logger.LogError("Gemini 请求失败: {ErrorMessage}", finalErrorMessage);

            await requestLogService.RecordFailure(
                logId,
                stopwatch,
                context.Response.StatusCode,
                finalErrorMessage);

            await context.Response.WriteAsync(finalErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini 请求处理过程中发生未捕获的异常");

            try
            {
                await requestLogService.RecordFailure(
                    logId,
                    stopwatch,
                    500,
                    $"未捕获的异常: {ex.Message}");
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "记录失败日志时发生异常");
            }

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"服务器内部错误: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 执行 Gemini 流式内容生成请求
    /// </summary>
    public async Task ExecuteStreamGenerateContent(
        HttpContext context,
        GeminiInput input,
        string model,
        string? conversationId,
        AIAccountService aiAccountService)
    {
        const int maxRetries = 15;
        string? lastErrorMessage = null;
        HttpStatusCode? lastStatusCode = null;

        // 创建请求日志
        var (logId, stopwatch) = await requestLogService.CreateRequestLog(
            context,
            model,
            false,
            null,
            false);

        bool sessionStickinessUsed = false;

        try
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (context.Response.HasStarted)
                {
                    logger.LogWarning("响应已开始写入，无法继续重试");
                    return;
                }

                logger.LogDebug("尝试第 {Attempt}/{MaxRetries} 次获取 Gemini 账户（流式）", attempt, maxRetries);

                // 会话粘性
                AIAccount? account = null;

                if (!string.IsNullOrEmpty(conversationId))
                {
                    var lastAccountId = quotaCache.GetConversationAccount(conversationId);
                    if (lastAccountId.HasValue)
                    {
                        account = await aiAccountService.TryGetAccountById(lastAccountId.Value);
                        if (account != null && account.Provider == AIProviders.Gemini)
                        {
                            sessionStickinessUsed = true;
                            logger.LogInformation(
                                "会话粘性成功：会话 {ConversationId} 复用 Gemini 账户 {AccountId}",
                                conversationId,
                                lastAccountId.Value);
                        }
                        else
                        {
                            account = null;
                        }
                    }
                }

                // 智能选择 Gemini 账户
                if (account == null)
                {
                    account = await aiAccountService.GetAIAccountByProvider(AIProviders.Gemini);
                }

                if (account == null)
                {
                    lastErrorMessage = $"无可用的 Gemini 账户（尝试 {attempt}/{maxRetries}）";
                    lastStatusCode = HttpStatusCode.ServiceUnavailable;

                    logger.LogWarning(lastErrorMessage);

                    if (!string.IsNullOrEmpty(lastErrorMessage))
                    {
                        await context.Response.WriteAsJsonAsync(lastErrorMessage);
                        return;
                    }

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    break;
                }

                var geminiOAuth = account.GetGeminiOauth();
                if (geminiOAuth == null)
                {
                    lastErrorMessage = $"账户 {account.Id} 没有有效的 Gemini OAuth 凭证";
                    lastStatusCode = HttpStatusCode.Unauthorized;

                    logger.LogWarning(lastErrorMessage);

                    if (attempt < maxRetries)
                    {
                        continue;
                    }

                    break;
                }

                // 发送流式请求
                try
                {
                    var codeAssistEndpoint = configuration["Gemini:CodeAssistEndpoint"] ??
                                             throw new InvalidOperationException(
                                                 "Gemini CodeAssistEndpoint is not configured");
                    var action = "streamGenerateContent";
                    var url = $"{codeAssistEndpoint}/v1internal:{action}?alt=sse";

                    var response = await SendGeminiRequest(
                        url,
                        input,
                        geminiOAuth.Token,
                        geminiOAuth.ProjectId,
                        model,
                        isStream: true);

                    await requestLogService.UpdateRetry(logId, attempt, account.Id);

                    // 处理响应
                    if (response.StatusCode >= HttpStatusCode.BadRequest)
                    {
                        var error = await response.Content.ReadAsStringAsync();

                        lastErrorMessage = error;
                        lastStatusCode = response.StatusCode;

                        logger.LogError(
                            "Gemini 流式请求失败 (账户: {AccountId}, 状态码: {StatusCode}): {Error}",
                            account.Id,
                            response.StatusCode,
                            error);

                        if (attempt < maxRetries)
                        {
                            continue;
                        }

                        await requestLogService.RecordFailure(
                            logId,
                            stopwatch,
                            (int)response.StatusCode,
                            error);

                        break;
                    }

                    // 成功响应
                    logger.LogInformation(
                        "成功处理 Gemini 流式请求 (账户: {AccountId}, 模型: {Model})",
                        account.Id,
                        model);

                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        quotaCache.SetConversationAccount(conversationId, account.Id);
                    }

                    await requestLogService.RecordSuccess(
                        logId,
                        stopwatch,
                        (int)response.StatusCode,
                        timeToFirstByteMs: stopwatch.ElapsedMilliseconds);

                    // 流式传输响应
                    context.Response.ContentType = "text/event-stream;charset=utf-8";
                    context.Response.Headers.TryAdd("Cache-Control", "no-cache");
                    context.Response.Headers.TryAdd("Connection", "keep-alive");
                    context.Response.StatusCode = (int)response.StatusCode;

                    try
                    {
                        await using var stream = await response.Content.ReadAsStreamAsync(context.RequestAborted);
                        await stream.CopyToAsync(context.Response.Body, context.RequestAborted);
                        await context.Response.Body.FlushAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("客户端取消 Gemini 流式请求 (账户: {AccountId})", account.Id);
                        return;
                    }
                    catch (Exception streamEx)
                    {
                        logger.LogError(streamEx,
                            "Gemini 流式传输过程中发生异常 (账户: {AccountId})",
                            account.Id);
                        return;
                    }

                    return;
                }
                catch (Exception ex)
                {
                    lastErrorMessage = $"请求异常 (账户: {account.Id}): {ex.Message}";
                    lastStatusCode = HttpStatusCode.InternalServerError;

                    logger.LogError(ex, "Gemini 流式请求异常（尝试 {Attempt}/{MaxRetries}）", attempt, maxRetries);

                    if (attempt < maxRetries)
                    {
                        continue;
                    }

                    await requestLogService.RecordFailure(
                        logId,
                        stopwatch,
                        (int)lastStatusCode,
                        lastErrorMessage);

                    break;
                }
            }

            // 所有重试都失败
            context.Response.StatusCode = (int)(lastStatusCode ?? HttpStatusCode.ServiceUnavailable);

            var finalErrorMessage = lastErrorMessage ?? $"所有 {maxRetries} 次重试均失败，无法完成请求";

            logger.LogError("Gemini 流式请求失败: {ErrorMessage}", finalErrorMessage);

            await requestLogService.RecordFailure(
                logId,
                stopwatch,
                context.Response.StatusCode,
                finalErrorMessage);

            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsync(finalErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini 流式请求处理过程中发生未捕获的异常");

            try
            {
                await requestLogService.RecordFailure(
                    logId,
                    stopwatch,
                    500,
                    $"未捕获的异常: {ex.Message}");
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "记录失败日志时发生异常");
            }

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"服务器内部错误: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 发送 Gemini API 请求
    /// </summary>
    private async Task<HttpResponseMessage> SendGeminiRequest(
        string url,
        GeminiInput input,
        string accessToken,
        string? projectId,
        string model,
        bool isStream)
    {
        // 构造符合Gemini内部API格式的请求体
        var geminiPayload = new
        {
            model = model,
            project = projectId ?? throw new InvalidOperationException("Project ID is required for Gemini API"),
            request = input
        };

        // 序列化为JSON
        var jsonPayload = JsonSerializer.Serialize(geminiPayload);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        // 添加请求头
        requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
        requestMessage.Headers.Add("User-Agent", GetUserAgent());

        return await HttpClient.SendAsync(requestMessage,
            isStream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead);
    }

    /// <summary>
    /// 获取与 Gemini CLI 一致的 User-Agent
    /// </summary>
    private static string GetUserAgent()
    {
        const string cliVersion = "0.1.5";
        var system = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows) ? "Windows" :
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux) ? "Linux" :
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX) ? "Darwin" : "Unknown";

        var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
        return $"GeminiCLI/{cliVersion} ({system}; {arch})";
    }
}