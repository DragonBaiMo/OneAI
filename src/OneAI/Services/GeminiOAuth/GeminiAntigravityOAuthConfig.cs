namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google Gemini Antigravity OAuth2 配置
/// Antigravity 是 Google 内部使用的特殊端点,具有更高的权限和不同的 API 访问方式
/// </summary>
public static class GeminiAntigravityOAuthConfig
{
    /// <summary>
    /// Google OAuth2 授权端点（与标准模式相同）
    /// </summary>
    public const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";

    /// <summary>
    /// Google OAuth2 Token 端点（与标准模式相同）
    /// </summary>
    public const string TokenUrl = "https://oauth2.googleapis.com/token";

    /// <summary>
    /// Google 用户信息端点（与标准模式相同）
    /// </summary>
    public const string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

    /// <summary>
    /// Antigravity API 端点 - 用于调用 Gemini API
    /// </summary>
    public const string AntigravityApiUrl = "https://cloudcode-pa.googleapis.com";

    /// <summary>
    /// 默认重定向 URI
    /// </summary>
    public const string RedirectUri = "http://localhost:8080/oauth-callback";

    /// <summary>
    /// Antigravity OAuth2 Client ID
    /// 这是专门用于 Antigravity 模式的 Client ID
    /// </summary>
    public static string ClientId => Environment.GetEnvironmentVariable("ANTIGRAVITY_CLIENT_ID")
        ?? "1071006060591-tmhssin2h21lcre235vtolojh4g403ep.apps.googleusercontent.com";

    /// <summary>
    /// Antigravity OAuth2 Client Secret
    /// </summary>
    public static string ClientSecret => Environment.GetEnvironmentVariable("ANTIGRAVITY_CLIENT_SECRET")
        ?? "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";

    /// <summary>
    /// Antigravity 所需的 OAuth2 Scopes
    /// 与标准模式相比,增加了 cclog 和 experimentsandconfigs
    /// </summary>
    public static readonly string[] Scopes =
    [
        "https://www.googleapis.com/auth/cloud-platform",
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/userinfo.profile",
        "https://www.googleapis.com/auth/cclog",
        "https://www.googleapis.com/auth/experimentsandconfigs"
    ];

    /// <summary>
    /// Antigravity User-Agent
    /// 必须使用特定的 User-Agent 才能访问 Antigravity 端点
    /// </summary>
    public const string UserAgent = "antigravity/1.11.3 windows/amd64";

    /// <summary>
    /// 获取以空格分隔的 Scopes 字符串
    /// </summary>
    public static string GetScopesString() => string.Join(" ", Scopes);
}
