using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using ServerLibrary.Services;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Server;

public class GameServerHandler(
    ILogger<GameServerHandler> logger,
    PacketHandler packetHandler, 
    SessionService sessionService) 
    : SimpleChannelInboundHandler<StreamPacket>
{
    private static readonly TimeSpan AuthTimeout = TimeSpan.FromSeconds(10);
    
    protected override void ChannelRead0(IChannelHandlerContext context, StreamPacket message)
    {
        _ = packetHandler.DispatchAsync(context, message)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogInformation($"[Error] {message.HeaderType}: {t.Exception?.InnerException?.Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
    }
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        var session = sessionService.AddSession(context);
        session.StartAuthTimeout(AuthTimeout);
        logger.LogInformation($"[Server] Connect Client: {context.Channel.RemoteAddress}");
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        sessionService.TryRemoveSession(context);
        logger.LogInformation($"[Server] Disconnect Client: {context.Channel.RemoteAddress}");
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        logger.LogError(exception.ToString());
        sessionService.TryRemoveSession(context);
        context.CloseAsync();
    }
}