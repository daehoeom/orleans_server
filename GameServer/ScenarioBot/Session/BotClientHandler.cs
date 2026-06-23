using DotNetty.Transport.Channels;
using SharedLibrary.Packet.Base;

namespace ScenarioBot.Session;

public class BotClientHandler(BotClientSession session) 
    : SimpleChannelInboundHandler<StreamPacket>
{
    public override void ChannelActive(IChannelHandlerContext ctx)
    {
        session.Attach(ctx.Channel);
    }
 
    public override void ChannelInactive(IChannelHandlerContext ctx)
    {
        session.OnDisconnected();
    }
 
    protected override void ChannelRead0(IChannelHandlerContext ctx, StreamPacket packet)
    {
        session.OnReceive(packet);
    }
 
    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
        Console.WriteLine($"[BotClientHandler] Bot#{session.BotId} 예외: {e.Message}");
        
        ctx.CloseAsync();
    }
}