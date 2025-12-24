using OneAI.Models;

namespace OneAI.Services.OpenAIOAuth;



/// <summary>
///     处理OpenAI OAuth授权码请求模型
/// </summary>
public record ExchangeOpenAiOAuthCodeRequest(
    string AuthorizationCode,
    string SessionId, // 用于从缓存中获取OAuth会话数据
    string? AccountName = null,
    string? Description = null,
    string? AccountType = null,
    int? Priority = null,
    ProxyConfig? Proxy = null
);