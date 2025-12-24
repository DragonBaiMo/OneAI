namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     OpenAI OAuth令牌响应
/// </summary>
public class OpenAiTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string TokenType { get; set; } = "Bearer";
}