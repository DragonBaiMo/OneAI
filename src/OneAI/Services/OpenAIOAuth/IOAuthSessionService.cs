namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     OAuth会话数据管理服务
/// </summary>
public interface IOAuthSessionService
{
    /// <summary>
    ///     存储OAuth会话数据
    /// </summary>
    void StoreSession(string sessionId, OAuthSessionData sessionData);

    /// <summary>
    ///     获取OAuth会话数据
    /// </summary>
    OAuthSessionData? GetSession(string sessionId);

    /// <summary>
    ///     移除OAuth会话数据
    /// </summary>
    void RemoveSession(string sessionId);

    /// <summary>
    ///     清理过期的会话数据
    /// </summary>
    void CleanupExpiredSessions();
}
