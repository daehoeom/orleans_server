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
            HeaderType = ResponseHeaderCache<T>.HeaderType,
            ResultCode = resultCode,
            Stream     = stream,
        };

        session.Channel.WriteAndFlushAsync(packet);
        return Task.CompletedTask;
    }

    protected static Task NotifyAsync<T>(PlayerSession session, T ntf)
        where T : class
    {
        var packet = new BaseNtfPacket<T>
        {
            HeaderType = NotifyHeaderCache<T>.HeaderType,
            Stream = ntf,
        };

        session.Channel.WriteAndFlushAsync(packet);
        return Task.CompletedTask;
    }
}