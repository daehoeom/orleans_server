using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ServerLibrary.Models;

namespace ServerLibrary.Services;

public class SessionService(ILogger<SessionService> logger)
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