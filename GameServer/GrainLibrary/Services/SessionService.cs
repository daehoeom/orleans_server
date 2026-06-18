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
    
    public bool AddSession(IChannelHandlerContext context)
    {
        if (!_sessions.TryAdd(context, new PlayerSession()))
        {
            logger.LogError($"Failed to add session in session service.");
            return false;
        }

        return true;
    }
}