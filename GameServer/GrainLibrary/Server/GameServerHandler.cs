using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using ServerLibrary.Services;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Server;

public class GameServerHandler : SimpleChannelInboundHandler<StreamPacket>
{
    private readonly PacketHandler _packetHandler;

    public GameServerHandler(PacketHandler packetHandler)
    {
        _packetHandler = packetHandler;
    }
    
    protected override void ChannelRead0(IChannelHandlerContext context, StreamPacket message)
    {
        _ = _packetHandler.DispatchAsync(context, message)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {

                }
            }, TaskContinuationOptions.OnlyOnFaulted);
    }
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine($"[Server] Connect Client: {context.Channel.RemoteAddress}");
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Console.WriteLine($"[Server] Disconnect Client: {context.Channel.RemoteAddress}");
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine(exception.ToString());
        context.CloseAsync();
    }
}