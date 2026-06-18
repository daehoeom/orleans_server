using System.Reflection;
using DotNetty.Transport.Channels;
using MessagePack;
using ServerLibrary.Models;
using SharedLibrary;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Services;

public static class HeaderCache<T>
    where T : class
{
    public static readonly PacketHeaderType HeaderType =
        typeof(T).GetCustomAttribute<ResponseAttribute>()?.HeaderType
        ?? throw new InvalidOperationException($"[ResponseHeader] 어트리뷰트가 없습니다: {typeof(T).Name}");
}

public abstract class PlayerBaseController(IClusterClient clusterClient)
{
    protected IClusterClient _clusterClient { get; } = clusterClient;

    protected static Task SendAsync<T>(PlayerSession session, T stream,  
        ResultCode resultCode = ResultCode.Success)
        where T : class, new()
    {
        var packet = new BaseResponsePacket<T>
        {
            HeaderType = HeaderCache<T>.HeaderType,
            ResultCode = resultCode,
            Stream     = stream,
        };
 
        return WriteAsync(session, packet);
    }

    protected static Task NotifyAsync<T>(PlayerSession session, T ntf)
        where T : class
    {
        var packet = new BaseNtfPacket<T>
        {
            HeaderType = HeaderCache<T>.HeaderType,
            Stream = ntf,
        };
 
        return WriteAsync(session, packet);
    }
    
    private static async Task WriteAsync<T>(PlayerSession session, T packet)
    {
        var bytes = MessagePackSerializer.Serialize(packet);
        var buffer   = session.Channel.Allocator.Buffer(bytes.Length);
        buffer.WriteBytes(bytes);
        await session.Channel.WriteAndFlushAsync(buffer);
    }
}