using OneAI.Models;

namespace OneAI.Services.OpenAIOAuth;

/// <summary>
///     生成OpenAI OAuth授权URL请求模型
/// </summary>
public record GenerateOpenAiOAuthUrlRequest(
    string? RedirectUri = null,
    ProxyConfig? Proxy = null
);