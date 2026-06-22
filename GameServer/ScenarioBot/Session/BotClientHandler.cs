using DotNetty.Transport.Channels;
using SharedLibrary.Packet.Base;

namespace ScenarioBot.Session;

public class BotClientHandler(BotClientSession session) 
    : SimpleChannelInboundHandler<StreamPacket>
{
    public override void ChannelActive(IChannelHandlerContext ctx)
    {
    }
 
    public override void ChannelInactive(IChannelHandlerContext ctx)
    {
    }
 
    protected override void ChannelRead0(IChannelHandlerContext ctx, StreamPacket packet)
    {
    }
 
    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
        ctx.CloseAsync();
    }
}