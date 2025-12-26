namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google OAuth2 Token 响应模型
/// </summary>
public class GeminiTokenResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌（用于离线访问）
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// ID 令牌
    /// </summary>
    public string? IdToken { get; set; }

    /// <summary>
    /// Token 过期时间（Unix 时间戳）
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Token 类型（通常为 Bearer）
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 授权范围
    /// </summary>
    public string[]? Scopes { get; set; }
}
