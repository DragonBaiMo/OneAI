using OneAI.Models;

namespace OneAI.Services.GeminiOAuth;

/// <summary>
/// 生成 Gemini OAuth 授权URL请求模型
/// </summary>
public record GenerateGeminiOAuthUrlRequest(
    string? RedirectUri = null,
    ProxyConfig? Proxy = null
);
