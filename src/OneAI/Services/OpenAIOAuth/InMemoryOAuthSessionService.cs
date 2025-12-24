using System.Collections.Concurrent;

namespace OneAI.Services.OpenAIOAuth;


/// <summary>
///     基于内存的OAuth会话数据管理服务实现
/// </summary>
public class InMemoryOAuthSessionService : IOAuthSessionService
{
    private readonly Timer _cleanupTimer;
    private readonly ConcurrentDictionary<string, OAuthSessionData> _sessions = new();

    public InMemoryOAuthSessionService()
    {
        // 每5分钟清理一次过期会话
        _cleanupTimer = new Timer(
            _ => CleanupExpiredSessions(),
            null,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5));
    }

    public void StoreSession(string sessionId, OAuthSessionData sessionData)
    {
        _sessions[sessionId] = sessionData;
    }

    public OAuthSessionData? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var sessionData))
        {
            // 检查是否过期
            if (sessionData.ExpiresAt > DateTime.UtcNow) return sessionData;

            // 过期则移除
            _sessions.TryRemove(sessionId, out _);
        }

        return null;
    }

    public void RemoveSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    public void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _sessions
            .Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys) _sessions.TryRemove(key, out _);
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}