using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using OneAI.Models;

namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google Gemini OAuth 辅助类
/// </summary>
public class GeminiOAuthHelper(ILogger<GeminiOAuthHelper> logger)
{
    /// <summary>
    /// 生成随机状态参数
    /// </summary>
    public string GenerateState()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// 生成 OAuth 授权 URL
    /// </summary>
    public string GenerateAuthUrl(string state, string? customRedirectUri = null)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["access_type"] = "offline";
        queryParams["client_id"] = GeminiOAuthConfig.ClientId;
        queryParams["prompt"] = "consent";
        queryParams["redirect_uri"] = customRedirectUri ?? GeminiOAuthConfig.RedirectUri;
        queryParams["response_type"] = "code";
        queryParams["scope"] = GeminiOAuthConfig.GetScopesString();
        queryParams["state"] = state;

        return $"{GeminiOAuthConfig.AuthorizeUrl}?{queryParams}";
    }

    /// <summary>
    /// 生成 OAuth 参数
    /// </summary>
    public GeminiOAuthParams GenerateOAuthParams(string? customRedirectUri = null)
    {
        var state = GenerateState();
        var authUrl = GenerateAuthUrl(state, customRedirectUri);

        return new GeminiOAuthParams
        {
            AuthUrl = authUrl,
            State = state
        };
    }

    /// <summary>
    /// 解析回调 URL 中的授权码
    /// </summary>
    public string ParseCallbackUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("请提供有效的授权码或回调 URL");

        var trimmedInput = input.Trim();

        // 如果输入是 URL，则从中提取 code 参数
        if (trimmedInput.StartsWith("http://") || trimmedInput.StartsWith("https://"))
            try
            {
                var uri = new Uri(trimmedInput);
                var query = HttpUtility.ParseQueryString(uri.Query);
                var authorizationCode = query["code"];

                if (string.IsNullOrEmpty(authorizationCode))
                    throw new ArgumentException("回调 URL 中未找到授权码 (code 参数)");

                return authorizationCode;
            }
            catch (UriFormatException)
            {
                throw new ArgumentException("无效的 URL 格式，请检查回调 URL 是否正确");
            }

        // 如果输入是纯授权码，直接返回
        var cleanedCode = trimmedInput.Split('#')[0]?.Split('&')[0] ?? trimmedInput;

        if (string.IsNullOrEmpty(cleanedCode) || cleanedCode.Length < 10)
            throw new ArgumentException("授权码格式无效，请确保复制了完整的 Authorization Code");

        return cleanedCode;
    }

    /// <summary>
    /// 使用授权码交换 Token
    /// </summary>
    public async Task<GeminiTokenResponse> ExchangeCodeForTokensAsync(
        string authorizationCode,
        string state,
        string? customRedirectUri = null,
        ProxyConfig? proxyConfig = null)
    {
        var cleanedCode = ParseCallbackUrl(authorizationCode);

        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", cleanedCode),
            new("redirect_uri", customRedirectUri ?? GeminiOAuthConfig.RedirectUri),
            new("client_id", GeminiOAuthConfig.ClientId),
            new("client_secret", GeminiOAuthConfig.ClientSecret)
        };

        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        try
        {
            logger.LogDebug("🔄 Attempting Google OAuth token exchange");

            var content = new FormUrlEncodedContent(parameters);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OneAI-OAuth/1.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.PostAsync(GeminiOAuthConfig.TokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("❌ Google OAuth token exchange failed: HTTP {Status} - {Error}",
                    (int)response.StatusCode, errorContent);
                throw new Exception($"Token exchange failed: HTTP {(int)response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            logger.LogInformation("✅ Google OAuth token exchange successful");

            var accessToken = root.GetProperty("access_token").GetString() ?? "";
            var refreshToken = root.TryGetProperty("refresh_token", out var refreshElement)
                ? refreshElement.GetString() ?? ""
                : "";
            var idToken = root.TryGetProperty("id_token", out var idElement)
                ? idElement.GetString() ?? ""
                : "";
            var expiresIn = root.TryGetProperty("expires_in", out var expiresElement)
                ? expiresElement.GetInt64()
                : 3600;

            return new GeminiTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdToken = idToken,
                ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + expiresIn,
                Scopes = GeminiOAuthConfig.Scopes,
                TokenType = "Bearer"
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError("❌ Google OAuth token exchange failed with network error: {Message}", ex.Message);
            throw new Exception("Token exchange failed: Network error or timeout");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError("❌ Google OAuth token exchange timed out: {Message}", ex.Message);
            throw new Exception("Token exchange failed: Request timed out");
        }
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    public async Task<GeminiTokenResponse> RefreshTokenAsync(
        string refreshToken,
        ProxyConfig? proxyConfig = null)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken),
            new("client_id", GeminiOAuthConfig.ClientId),
            new("client_secret", GeminiOAuthConfig.ClientSecret)
        };

        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        try
        {
            logger.LogDebug("🔄 Attempting Google OAuth token refresh");

            var content = new FormUrlEncodedContent(parameters);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OneAI-OAuth/1.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.PostAsync(GeminiOAuthConfig.TokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("❌ Google OAuth token refresh failed: HTTP {Status} - {Error}",
                    (int)response.StatusCode, errorContent);
                throw new Exception($"Token refresh failed: HTTP {(int)response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            logger.LogInformation("✅ Google OAuth token refresh successful");

            var accessToken = root.GetProperty("access_token").GetString() ?? "";
            var expiresIn = root.TryGetProperty("expires_in", out var expiresElement)
                ? expiresElement.GetInt64()
                : 3600;

            return new GeminiTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken, // 保留原有的 refresh token
                ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + expiresIn,
                Scopes = GeminiOAuthConfig.Scopes,
                TokenType = "Bearer"
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError("❌ Google OAuth token refresh failed with network error: {Message}", ex.Message);
            throw new Exception("Token refresh failed: Network error or timeout");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError("❌ Google OAuth token refresh timed out: {Message}", ex.Message);
            throw new Exception("Token refresh failed: Request timed out");
        }
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    public async Task<GeminiUserInfo> GetUserInfoAsync(string accessToken, ProxyConfig? proxyConfig = null)
    {
        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        try
        {
            logger.LogDebug("🔄 Fetching Google user info");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OneAI-OAuth/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.GetAsync(GeminiOAuthConfig.UserInfoUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("❌ Failed to get Google user info: HTTP {Status} - {Error}",
                    (int)response.StatusCode, errorContent);
                throw new Exception($"Failed to get user info: HTTP {(int)response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GeminiUserInfo>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            logger.LogInformation("✅ Successfully fetched Google user info");
            return userInfo ?? new GeminiUserInfo();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError("❌ Failed to get Google user info: Network error - {Message}", ex.Message);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError("❌ Failed to get Google user info: Timeout - {Message}", ex.Message);
            throw new Exception("Request timed out");
        }
    }

    /// <summary>
    /// 从 API 自动获取项目ID（标准模式）
    /// </summary>
    public async Task<string?> FetchProjectIdAsync(string accessToken, ProxyConfig? proxyConfig = null)
    {
        var headers = new Dictionary<string, string>
        {
            ["User-Agent"] = "GeminiCLI/0.1.5 (Windows; AMD64)",
            ["Authorization"] = $"Bearer {accessToken}"
        };

        // 步骤 1: 尝试 loadCodeAssist
        try
        {
            logger.LogInformation("标准模式：从 loadCodeAssist API 获取 project_id...");
            var projectId = await TryLoadCodeAssistAsync(GeminiOAuthConfig.CodeAssistEndpoint, headers, proxyConfig);
            if (!string.IsNullOrEmpty(projectId))
            {
                logger.LogInformation("✅ 成功从 loadCodeAssist API 获取 project_id: {ProjectId}", projectId);
                return projectId;
            }

            logger.LogWarning("⚠️ loadCodeAssist 未返回 project_id，回退到 onboardUser");
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ loadCodeAssist 失败: {Message}", ex.Message);
            logger.LogWarning("回退到 onboardUser");
        }

        // 步骤 2: 回退到 onboardUser
        try
        {
            var projectId = await TryOnboardUserAsync(GeminiOAuthConfig.CodeAssistEndpoint, headers, proxyConfig);
            if (!string.IsNullOrEmpty(projectId))
            {
                logger.LogInformation("✅ 成功从 onboardUser API 获取 project_id: {ProjectId}", projectId);
                return projectId;
            }

            logger.LogError("❌ 从 loadCodeAssist 和 onboardUser 都无法获取 project_id");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError("❌ onboardUser 失败: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 尝试通过 loadCodeAssist 获取项目ID
    /// </summary>
    private async Task<string?> TryLoadCodeAssistAsync(
        string apiBaseUrl,
        Dictionary<string, string> headers,
        ProxyConfig? proxyConfig = null)
    {
        var requestUrl = $"{apiBaseUrl.TrimEnd('/')}/v1internal:loadCodeAssist";
        var requestBody = new
        {
            metadata = new
            {
                ideType = "ANTIGRAVITY",
                platform = "PLATFORM_UNSPECIFIED",
                pluginType = "GEMINI"
            }
        };

        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        logger.LogDebug("[loadCodeAssist] 从 {Url} 获取 project_id", requestUrl);

        httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in headers)
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(requestUrl, content);

        logger.LogDebug("[loadCodeAssist] 响应状态: {Status}", (int)response.StatusCode);

        if (response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            logger.LogDebug("[loadCodeAssist] 响应内容: {Response}", responseText);

            using var document = JsonDocument.Parse(responseText);
            var root = document.RootElement;

            // 检查是否有 currentTier（表示用户已激活）
            if (root.TryGetProperty("currentTier", out var currentTier))
            {
                logger.LogInformation("[loadCodeAssist] 用户已激活");

                // 使用服务器返回的 project_id
                if (root.TryGetProperty("cloudaicompanionProject", out var projectElement))
                {
                    var projectId = projectElement.GetString();
                    if (!string.IsNullOrEmpty(projectId))
                    {
                        logger.LogInformation("[loadCodeAssist] 成功获取 project_id: {ProjectId}", projectId);
                        return projectId;
                    }
                }

                logger.LogWarning("[loadCodeAssist] 响应中没有 project_id");
                return null;
            }

            logger.LogInformation("[loadCodeAssist] 用户未激活（没有 currentTier）");
            return null;
        }

        var errorText = await response.Content.ReadAsStringAsync();
        logger.LogWarning("[loadCodeAssist] 失败: HTTP {Status}", (int)response.StatusCode);
        logger.LogWarning("[loadCodeAssist] 响应内容: {Response}", errorText.Length > 500 ? errorText.Substring(0, 500) : errorText);
        throw new Exception($"HTTP {(int)response.StatusCode}: {(errorText.Length > 200 ? errorText.Substring(0, 200) : errorText)}");
    }

    /// <summary>
    /// 尝试通过 onboardUser 获取项目ID（长时间运行操作，需要轮询）
    /// </summary>
    private async Task<string?> TryOnboardUserAsync(
        string apiBaseUrl,
        Dictionary<string, string> headers,
        ProxyConfig? proxyConfig = null)
    {
        var requestUrl = $"{apiBaseUrl.TrimEnd('/')}/v1internal:onboardUser";

        // 首先需要获取用户的 tier 信息
        var tierId = await GetOnboardTierAsync(apiBaseUrl, headers, proxyConfig);
        if (string.IsNullOrEmpty(tierId))
        {
            logger.LogError("[onboardUser] 无法确定用户 tier");
            return null;
        }

        logger.LogInformation("[onboardUser] 用户 tier: {TierId}", tierId);

        // 构造 onboardUser 请求
        var requestBody = new
        {
            tierId,
            metadata = new
            {
                ideType = "ANTIGRAVITY",
                platform = "PLATFORM_UNSPECIFIED",
                pluginType = "GEMINI"
            }
        };

        logger.LogDebug("[onboardUser] 请求 URL: {Url}", requestUrl);

        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        // onboardUser 是长时间运行操作，需要轮询
        const int maxAttempts = 5;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;
            logger.LogDebug("[onboardUser] 轮询尝试 {Attempt}/{Max}", attempt, maxAttempts);

            httpClient.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(requestUrl, content);

            logger.LogDebug("[onboardUser] 响应状态: {Status}", (int)response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseText);
                var root = document.RootElement;

                logger.LogDebug("[onboardUser] 响应数据: {Response}", responseText);

                // 检查长时间运行操作是否完成
                if (root.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
                {
                    logger.LogInformation("[onboardUser] 操作完成");

                    // 从响应中提取 project_id
                    if (root.TryGetProperty("response", out var responseData))
                    {
                        if (responseData.TryGetProperty("cloudaicompanionProject", out var projectElement))
                        {
                            string? projectId = null;

                            if (projectElement.ValueKind == JsonValueKind.Object)
                            {
                                if (projectElement.TryGetProperty("id", out var idElement))
                                    projectId = idElement.GetString();
                            }
                            else if (projectElement.ValueKind == JsonValueKind.String)
                            {
                                projectId = projectElement.GetString();
                            }

                            if (!string.IsNullOrEmpty(projectId))
                            {
                                logger.LogInformation("[onboardUser] 成功获取 project_id: {ProjectId}", projectId);
                                return projectId;
                            }
                        }
                    }

                    logger.LogWarning("[onboardUser] 操作完成但响应中没有 project_id");
                    return null;
                }

                logger.LogDebug("[onboardUser] 操作仍在进行中，等待 2 秒...");
                await Task.Delay(2000);
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                logger.LogWarning("[onboardUser] 失败: HTTP {Status}", (int)response.StatusCode);
                logger.LogWarning("[onboardUser] 响应内容: {Response}",
                    errorText.Length > 500 ? errorText.Substring(0, 500) : errorText);
                throw new Exception(
                    $"HTTP {(int)response.StatusCode}: {(errorText.Length > 200 ? errorText.Substring(0, 200) : errorText)}");
            }
        }

        logger.LogError("[onboardUser] 超时: 操作在 10 秒内未完成");
        return null;
    }

    /// <summary>
    /// 从 loadCodeAssist 响应中获取用户应该注册的 tier
    /// </summary>
    private async Task<string?> GetOnboardTierAsync(
        string apiBaseUrl,
        Dictionary<string, string> headers,
        ProxyConfig? proxyConfig = null)
    {
        var requestUrl = $"{apiBaseUrl.TrimEnd('/')}/v1internal:loadCodeAssist";
        var requestBody = new
        {
            metadata = new
            {
                ideType = "ANTIGRAVITY",
                platform = "PLATFORM_UNSPECIFIED",
                pluginType = "GEMINI"
            }
        };

        logger.LogDebug("[_get_onboard_tier] 从 {Url} 获取 tier 信息", requestUrl);

        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in headers)
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(requestUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseText);
            var root = document.RootElement;

            logger.LogDebug("[_get_onboard_tier] 响应数据: {Response}", responseText);

            // 查找默认的 tier
            if (root.TryGetProperty("allowedTiers", out var allowedTiers))
            {
                foreach (var tier in allowedTiers.EnumerateArray())
                {
                    if (tier.TryGetProperty("isDefault", out var isDefault) && isDefault.GetBoolean())
                    {
                        if (tier.TryGetProperty("id", out var idElement))
                        {
                            var tierId = idElement.GetString();
                            logger.LogInformation("[_get_onboard_tier] 找到默认 tier: {TierId}", tierId);
                            return tierId;
                        }
                    }
                }
            }

            // 如果没有默认 tier，使用 LEGACY 作为回退
            logger.LogWarning("[_get_onboard_tier] 未找到默认 tier，使用 LEGACY");
            return "LEGACY";
        }

        logger.LogError("[_get_onboard_tier] 获取 tier 信息失败: HTTP {Status}", (int)response.StatusCode);
        return null;
    }

    /// <summary>
    /// 获取 GCP 项目列表
    /// </summary>
    public async Task<List<GeminiProject>?> GetProjectsAsync(string accessToken, ProxyConfig? proxyConfig = null)
    {
        using var httpClient = CreateHttpClientWithProxy(proxyConfig);

        try
        {
            logger.LogDebug("🔄 Fetching Google Cloud projects");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "geminicli-oauth/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.GetAsync(GeminiOAuthConfig.ProjectsUrl);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("⚠️ Failed to fetch projects: HTTP {Status}",
                    (int)response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            var projects = new List<GeminiProject>();

            if (root.TryGetProperty("projects", out var projectsElement))
            {
                foreach (var projectJson in projectsElement.EnumerateArray())
                {
                    var project = new GeminiProject
                    {
                        ProjectId = projectJson.TryGetProperty("projectId", out var id)
                            ? id.GetString()
                            : null,
                        ProjectName = projectJson.TryGetProperty("name", out var name)
                            ? name.GetString()
                            : null,
                        ProjectNumber = projectJson.TryGetProperty("projectNumber", out var number)
                            ? number.GetString()
                            : null,
                        State = projectJson.TryGetProperty("lifecycleState", out var state)
                            ? state.GetString()
                            : null
                    };
                    projects.Add(project);
                }
            }

            logger.LogInformation("✅ Successfully fetched {Count} Google Cloud projects", projects.Count);
            return projects;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("⚠️ Failed to fetch projects: Network error - {Message}", ex.Message);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning("⚠️ Failed to fetch projects: Timeout - {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 格式化 Gemini 凭据
    /// </summary>
    public GeminiOauth FormatGeminiCredentials(GeminiTokenResponse tokenData, GeminiUserInfo userInfo,
        List<GeminiProject>? projects = null)
    {
        return new GeminiOauth
        {
            AccessToken = tokenData.AccessToken,
            RefreshToken = tokenData.RefreshToken,
            ExpiresAt = tokenData.ExpiresAt,
            Scopes = tokenData.Scopes,
            IsMax = true,
            UserInfo = userInfo,
            Projects = projects,
            ProjectId = projects?.FirstOrDefault()?.ProjectId
        };
    }

    /// <summary>
    /// 创建带代理的 HttpClient
    /// </summary>
    private HttpClient CreateHttpClientWithProxy(ProxyConfig? proxyConfig)
    {
        if (proxyConfig == null)
            return new HttpClient();

        try
        {
            var handler = new HttpClientHandler();
            var proxyUri = $"{proxyConfig.Type}://{proxyConfig.Host}:{proxyConfig.Port}";

            if (!string.IsNullOrEmpty(proxyConfig.Username) && !string.IsNullOrEmpty(proxyConfig.Password))
                proxyUri =
                    $"{proxyConfig.Type}://{proxyConfig.Username}:{proxyConfig.Password}@{proxyConfig.Host}:{proxyConfig.Port}";

            handler.Proxy = new WebProxy(proxyUri);
            handler.UseProxy = true;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate |  DecompressionMethods.Brotli;

            return new HttpClient(handler);
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Invalid proxy configuration: {Error}", ex.Message);
            return new HttpClient();
        }
    }
}

/// <summary>
/// Gemini OAuth 参数模型
/// </summary>
public class GeminiOAuthParams
{
    public required string AuthUrl { get; set; }
    public required string State { get; set; }
}
