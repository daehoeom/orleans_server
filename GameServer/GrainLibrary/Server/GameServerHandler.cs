using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using GrainLibrary.Services;
using SharedLibrary.Packet.Base;

namespace GrainLibrary.Server;

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
                    logger.LogError($"[Error] {message.HeaderType}: {t.Exception?.InnerException?.Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
        var session = sessionService.AddContext(context);
        if (session is null)
        {
            logger.LogError($"[Server] 세션 추가 실패, 연결 종료: {context.Channel.RemoteAddress}");
            context.CloseAsync();
            return;
        }

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