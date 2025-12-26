namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google Gemini OAuth2 配置
/// </summary>
public static class GeminiOAuthConfig
{
    /// <summary>
    /// Google OAuth2 授权端点
    /// </summary>
    public const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";

    /// <summary>
    /// Google OAuth2 Token 端点
    /// </summary>
    public const string TokenUrl = "https://oauth2.googleapis.com/token";

    /// <summary>
    /// Google 用户信息端点
    /// </summary>
    public const string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

    /// <summary>
    /// Google Cloud Resource Manager API（获取项目信息）
    /// </summary>
    public const string ProjectsUrl = "https://cloudresourcemanager.googleapis.com/v1/projects";

    /// <summary>
    /// Google Code Assist API 端点（用于自动获取项目ID）
    /// </summary>
    public const string CodeAssistEndpoint = "https://cloudcode-pa.googleapis.com";

    /// <summary>
    /// 默认重定向 URI
    /// </summary>
    public const string RedirectUri = "http://localhost:8080/oauth-callback";

    /// <summary>
    /// Google OAuth2 Client ID
    /// </summary>
    public static string ClientId => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
        ?? "1071006060591-tmhssin2h21lcre235vtolojh4g403ep.apps.googleusercontent.com";

    /// <summary>
    /// Google OAuth2 Client Secret
    /// </summary>
    public static string ClientSecret => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
        ?? "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";

    /// <summary>
    /// 所需的 OAuth2 Scopes
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
    /// 获取以空格分隔的 Scopes 字符串
    /// </summary>
    public static string GetScopesString() => string.Join(" ", Scopes);
}
