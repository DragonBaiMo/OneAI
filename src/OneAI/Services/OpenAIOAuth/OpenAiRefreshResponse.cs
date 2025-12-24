namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     OpenAI刷新令牌响应
/// </summary>
public class OpenAiRefreshResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    
    public string TokenType { get; set; } = "Bearer";
}
