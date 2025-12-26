namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Gemini OAuth 凭证响应模型，返回完整的OAuth凭据信息
/// </summary>
public class GeminiOAuthCredentialsDto
{
    /// <summary>
    /// OAuth 客户端 ID
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// OAuth 客户端密钥
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 授权范围
    /// </summary>
    public string[]? Scopes { get; set; }

    /// <summary>
    /// Token 服务器 URL
    /// </summary>
    public required string TokenUri { get; set; }

    /// <summary>
    /// GCP 项目 ID
    /// </summary>
    public required string ProjectId { get; set; }

    /// <summary>
    /// Token 过期时间 (ISO 8601 格式)
    /// </summary>
    public string? Expiry { get; set; }

    /// <summary>
    /// 是否自动检测的项目 ID
    /// </summary>
    public bool AutoDetectedProject { get; set; }
}
