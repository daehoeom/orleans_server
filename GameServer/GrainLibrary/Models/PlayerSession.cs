using DotNetty.Transport.Channels;

namespace ServerLibrary.Models;

public class PlayerSession
{
    private long _sessionId;
    private IChannelHandlerContext _handlerContext;
    
    public long SessionId => _sessionId;

    public PlayerSession CreateSession(IChannelHandlerContext context, long sessionId)
    {
        _handlerContext = context;
        _sessionId = sessionId;
    }
}