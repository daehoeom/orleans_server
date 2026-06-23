using MessagePack;
using ServerLibrary.Models;
using SharedLibrary;
using SharedLibrary.Packet;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Services;

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
        await session.Channel.WriteAndFlushAsync(packet);
    }
}