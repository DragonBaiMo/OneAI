namespace OneAI.Services.OpenAIOAuth;

/// <summary>
///     OpenAI OAuth配置
/// </summary>
public static class OpenAiOAuthConfig
{
    public const string AuthorizeUrl = "https://auth.openai.com/oauth/authorize";
    
    public const string TokenUrl = "https://auth.openai.com/oauth/token";
    
    public const string UserInfoUrl = "https://api.openai.com/v1/me";
    
    public const string ClientId = "app_EMoamEEZ73f0CkXaXp7hrann"; // 根据您的示例URL
    
    public const string RedirectUri = "http://localhost:1455/auth/callback"; // 根据您的示例URL
    
    public const string Scopes = "openid profile email offline_access";
}
