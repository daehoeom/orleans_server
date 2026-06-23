using System.Collections.Concurrent;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using ServerLibrary.Models;

namespace ServerLibrary.Services;

public class SessionService(ILogger<SessionService> logger)
{
    private ConcurrentDictionary<IChannelHandlerContext, PlayerSession> _sessions = new();

    public PlayerSession? GetSession(IChannelHandlerContext context)
    {
        return _sessions.TryGetValue(context, out var session) ? session : null;
    }

    public List<PlayerSession> GetAllSession()
    {
        return _sessions.Values.ToList();
    }
    
    public PlayerSession AddSession(IChannelHandlerContext context)
    {
        var addSession = new PlayerSession
        {
            Channel = context,
        };
        if (!_sessions.TryAdd(context, addSession))
        {
            logger.LogError($"Failed to add session in session service.");
            return null;
        }

        return addSession;
    }

    public bool TryRemoveSession(IChannelHandlerContext context)
    {
        if (!_sessions.TryRemove(context, out var session))
        {
            session?.Dispose();
            return false;
        }

        return true;
    }

    public void Broadcast<T>(T packet)
        where T : class
    {
        foreach (var session in _sessions.Values)
        {
            session.
        }
    }
}