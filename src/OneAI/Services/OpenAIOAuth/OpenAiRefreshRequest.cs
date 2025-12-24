namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     OpenAI刷新令牌请求
/// </summary>
public class OpenAiRefreshRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string GrantType { get; set; } = "refresh_token";
    public string RefreshToken { get; set; } = string.Empty;
    public string Scope { get; set; } = "openid profile email";
}