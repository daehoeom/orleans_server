using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ServerLibrary.Models;

namespace ServerLibrary.Manager;

public class SessionManager(ILogger<SessionManager> logger)
    : SingletonBase<SessionManager>
{
    private ConcurrentDictionary<long, PlayerSession> _sessions = new();
    
    public bool AddSession(long sessionId)
    {
        if (!_sessions.TryAdd(sessionId, new PlayerSession()))
        {
            logger.LogError($"Failed to add session in session service.");
            return false;
        }

        return true;
    }
}