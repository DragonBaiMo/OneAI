using OneAI.Models;

namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// 处理 Gemini OAuth 授权码请求模型
/// </summary>
public record ExchangeGeminiOAuthCodeRequest(
    string AuthorizationCode,
    string SessionId, // 用于从缓存中获取OAuth会话数据
    string? AccountName = null,
    string? Description = null,
    string? AccountType = null,
    int? Priority = null,
    string? ProjectId = null, // 可选的 GCP 项目 ID，如果不提供则自动检测
    ProxyConfig? Proxy = null
);

