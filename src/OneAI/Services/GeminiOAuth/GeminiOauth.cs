namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Gemini OAuth 凭证存储模型
/// </summary>
public class GeminiOauth
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token 过期时间（Unix 时间戳）
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// 授权范围
    /// </summary>
    public string[]? Scopes { get; set; }

    /// <summary>
    /// 是否为最大权限账户
    /// </summary>
    public bool IsMax { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public GeminiUserInfo? UserInfo { get; set; }

    /// <summary>
    /// 关联的 GCP 项目列表
    /// </summary>
    public List<GeminiProject>? Projects { get; set; }

    /// <summary>
    /// 项目 ID（从Projects列表中选择）
    /// </summary>
    public string? ProjectId { get; set; }
}

/// <summary>
/// Gemini 关联的 GCP 项目信息
/// </summary>
public class GeminiProject
{
    /// <summary>
    /// 项目 ID
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// 项目编号
    /// </summary>
    public string? ProjectNumber { get; set; }

    /// <summary>
    /// 项目生命周期状态（ACTIVE, DELETE_REQUESTED 等）
    /// </summary>
    public string? State { get; set; }
}
