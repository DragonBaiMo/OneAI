namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// Google 用户信息模型
/// </summary>
public class GeminiUserInfo
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 邮箱是否已验证
    /// </summary>
    public bool VerifiedEmail { get; set; }

    /// <summary>
    /// 用户名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 用户头像 URL
    /// </summary>
    public string? Picture { get; set; }

    /// <summary>
    /// 姓氏
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// 用户语言区域设置
    /// </summary>
    public string? Locale { get; set; }
}
