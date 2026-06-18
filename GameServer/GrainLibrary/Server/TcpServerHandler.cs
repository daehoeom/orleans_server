using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using ServerLibrary.Manager;

namespace ServerLibrary.Server;

public class TcpServerHandler : SimpleChannelInboundHandler<IByteBuffer>
{
    protected override void ChannelRead0(IChannelHandlerContext context, IByteBuffer message)
    {
        var received = message.ToString(Encoding.UTF8);
        Console.WriteLine($"[Server] Receive data : {received}");
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