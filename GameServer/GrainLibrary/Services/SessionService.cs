using System.Collections.Concurrent;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using GrainLibrary.Models;

namespace GrainLibrary.Services;

public class SessionService(ILogger<SessionService> logger)
{
    private ConcurrentDictionary<IChannelHandlerContext, PlayerSession> _contextSessions = new();
    private ConcurrentDictionary<long, PlayerSession> _sessions = new();

    public PlayerSession? GetSession(IChannelHandlerContext context)
    {
        return _contextSessions.TryGetValue(context, out var session) ? session : null;
    }

    public PlayerSession? GetSession(long sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    public List<PlayerSession> GetAllSession()
    {
        return _sessions.Values.ToList();
    }

    public PlayerSession? AddSession(PlayerSession session)
    {
        return _sessions.TryAdd(session.SessionId, session) ? session : null;
    }

    public PlayerSession? AddContext(IChannelHandlerContext context)
    {
        var addSession = new PlayerSession
        {
            Channel = context,
        };

        if (_contextSessions.TryAdd(context, addSession))
        {
            return addSession;
        }
        
        logger.LogError($"Failed to add session in session service.");
        return null;
    }

    public bool TryRemoveSession(IChannelHandlerContext context)
    {
        if (!_contextSessions.TryRemove(context, out var session))
        {
            return false;
        }

        _sessions.TryRemove(session.SessionId, out _);

        session.Dispose();
        return true;
    }

    public void Broadcast<T>(T body)
        where T : class, new()
    {
        foreach (var session in _sessions.Values)
        {
            session.Notify(body);
        }
    }
}