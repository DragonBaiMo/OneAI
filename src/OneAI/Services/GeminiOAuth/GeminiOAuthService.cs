using OneAI.Constants;
using OneAI.Data;
using OneAI.Entities;
using OneAI.Services.OpenAIOAuth;

namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google Gemini OAuth 服务
/// </summary>
public class GeminiOAuthService(
    GeminiOAuthHelper authHelper,
    AppDbContext appDbContext,
    AccountQuotaCacheService quotaCacheService)
{
    /// <summary>
    /// 生成 Gemini OAuth 授权链接
    /// </summary>
    public object GenerateGeminiOAuthUrl(
        GenerateGeminiOAuthUrlRequest request,
        GeminiOAuthHelper oAuthHelper,
        IOAuthSessionService sessionService)
    {
        // 如果前端没有传递 RedirectUri，使用默认配置
        var redirectUri = string.IsNullOrEmpty(request.RedirectUri)
            ? GeminiOAuthConfig.RedirectUri
            : request.RedirectUri;

        var oAuthParams = oAuthHelper.GenerateOAuthParams(redirectUri);

        // 将 OAuth 会话数据存储到缓存中，用于后续验证
        var sessionData = new OAuthSessionData
        {
            CodeVerifier = oAuthParams.State, // 对于 Gemini，不需要 CodeVerifier，用 State 替代
            State = oAuthParams.State,
            CodeChallenge = oAuthParams.State, // 对于 Gemini，不需要 CodeChallenge，用 State 替代
            Proxy = request.Proxy,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(10) // 10 分钟过期
        };

        // 存储会话数据
        sessionService.StoreSession(oAuthParams.State, sessionData);

        return new
        {
            authUrl = oAuthParams.AuthUrl,
            sessionId = oAuthParams.State, // 使用 state 作为 sessionId
            state = oAuthParams.State,
            message = "请复制此链接到浏览器中进行授权，授权完成后将获得 Authorization Code"
        };
    }

    /// <summary>
    /// 处理 Gemini OAuth 授权码
    /// </summary>
    public async Task<AIAccount> ExchangeGeminiOAuthCode(
        AppDbContext dbContext,
        ExchangeGeminiOAuthCodeRequest request,
        GeminiOAuthHelper oAuthHelper,
        IOAuthSessionService sessionService)
    {
        // 从缓存中获取 OAuth 会话数据
        var sessionData = sessionService.GetSession(request.SessionId);
        if (sessionData == null)
        {
            throw new ArgumentException("OAuth 会话已过期或无效，请重新获取授权链接");
        }

        // 使用会话数据中的参数进行 token 交换
        var tokenResponse = await oAuthHelper.ExchangeCodeForTokensAsync(
            request.AuthorizationCode,
            sessionData.State,
            null, // 使用默认 RedirectUri
            sessionData.Proxy ?? request.Proxy);

        // 获取用户信息
        var userInfo = await oAuthHelper.GetUserInfoAsync(
            tokenResponse.AccessToken,
            sessionData.Proxy ?? request.Proxy);

        // 获取项目信息并进行自动检测（完全参考Python逻辑）
        string? detectedProjectId = null;
        bool autoDetected = false;

        // 检查是否已经提供了项目ID（Python: if not project_id）
        if (string.IsNullOrEmpty(request.ProjectId))
        {
            // 没有提供项目ID，需要自动检测
            // 步骤1：先尝试使用fetch_project_id从API获取项目ID
            try
            {
                detectedProjectId = await oAuthHelper.FetchProjectIdAsync(
                    tokenResponse.AccessToken,
                    sessionData.Proxy ?? request.Proxy);

                if (!string.IsNullOrEmpty(detectedProjectId))
                {
                    autoDetected = true;
                }
                else
                {
                    // 步骤2：如果API方式失败，回退到项目列表获取方式
                    var projects = await oAuthHelper.GetProjectsAsync(
                        tokenResponse.AccessToken,
                        sessionData.Proxy ?? request.Proxy);

                    if (projects != null && projects.Any())
                    {
                        // 无论一个还是多个项目，都选择第一个（完全匹配Python逻辑）
                        detectedProjectId = projects.First().ProjectId;
                        autoDetected = true;
                    }
                    else
                    {
                        // 没有项目访问权限
                        throw new InvalidOperationException(
                            "未检测到可访问的 GCP 项目，请检查权限或手动指定项目ID");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"自动检测项目ID失败: {ex.Message}，请手动指定项目ID", ex);
            }
        }
        else
        {
            // 使用提供的项目ID（Python: else: detected_project_id = project_id）
            detectedProjectId = request.ProjectId;
            autoDetected = false;
        }

        // 格式化过期时间为 ISO 8601 格式
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt);
        var expiryString = expiresAt.UtcDateTime.ToString("O"); // ISO 8601 格式

        var account = new AIAccount
        {
            Provider = AIProviders.Gemini,
            ApiKey = string.Empty,
            BaseUrl = string.Empty,
            CreatedAt = DateTime.Now,
            Email = userInfo.Email,
            Name = userInfo.Name ?? request.AccountName,
            IsEnabled = true
        };

        account.SetGeminiOAuth(new GeminiOAuthCredentialsDto
        {
            ClientId = GeminiOAuthConfig.ClientId,
            ClientSecret = GeminiOAuthConfig.ClientSecret,
            Token = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            Scopes = tokenResponse.Scopes,
            TokenUri = GeminiOAuthConfig.TokenUrl,
            ProjectId = detectedProjectId ?? string.Empty,
            Expiry = expiryString,
            AutoDetectedProject = autoDetected
        });

        await dbContext.AIAccounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // 清理已使用的会话数据
        sessionService.RemoveSession(request.SessionId);

        return account;
    }

    /// <summary>
    /// 刷新 Gemini OAuth Token
    /// </summary>
    public async Task RefreshGeminiOAuthTokenAsync(AIAccount account)
    {
        var currentCredentials = account.GetGeminiOauth();
        if (string.IsNullOrEmpty(currentCredentials?.RefreshToken))
            throw new InvalidOperationException("没有可用的 Gemini 刷新令牌");

        // 使用 GeminiOAuthHelper 刷新令牌
        var refreshResponse = await authHelper.RefreshTokenAsync(currentCredentials.RefreshToken);

        // 格式化过期时间为 ISO 8601 格式
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(refreshResponse.ExpiresAt);
        var expiryString = expiresAt.UtcDateTime.ToString("O");

        // 更新凭据（保留原有的其他信息）
        account.SetGeminiOAuth(new GeminiOAuthCredentialsDto
        {
            ClientId = currentCredentials.ClientId,
            ClientSecret = currentCredentials.ClientSecret,
            Token = refreshResponse.AccessToken,
            RefreshToken = refreshResponse.RefreshToken,
            Scopes = refreshResponse.Scopes ?? currentCredentials.Scopes,
            TokenUri = currentCredentials.TokenUri,
            ProjectId = currentCredentials.ProjectId,
            Expiry = expiryString,
            AutoDetectedProject = currentCredentials.AutoDetectedProject
        });

        appDbContext.AIAccounts.Update(account);
        await appDbContext.SaveChangesAsync();

        // 清除账户列表缓存（因为账户的 OAuth 信息发生了变化）
        quotaCacheService.ClearAccountsCache();
    }
}