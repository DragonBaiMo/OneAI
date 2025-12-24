namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     OpenAI OAuth参数
/// </summary>
public class OpenAiOAuthParams
{
    public string AuthUrl { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CodeChallenge { get; set; } = string.Empty;
}