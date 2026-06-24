using MessagePack;
using GrainLibrary.Models;
using SharedLibrary;
using SharedLibrary.Packet;
using SharedLibrary.Packet.Base;

namespace GrainLibrary.Services;

public abstract class PlayerBaseController(IClusterClient clusterClient)
{
    protected IClusterClient _clusterClient { get; } = clusterClient;

    protected static Task SendAsync<T>(PlayerSession session, 
        ResultCode resultCode = ResultCode.Success, T? response = null)
        where T : class, new()
    {
        var packet = new BaseResponsePacket<T>
        {
            HeaderType = ResponseHeaderCache<T>.HeaderType,
            ResultCode = resultCode,
            Stream     = response ?? new T(),
        };

        return session.Channel.WriteAndFlushAsync(packet);
    }

    protected static Task NotifyAsync<T>(PlayerSession session, T ntf)
        where T : class
    {
        var packet = new BaseNtfPacket<T>
        {
            HeaderType = NotifyHeaderCache<T>.HeaderType,
            Stream = ntf,
        };

        return session.Channel.WriteAndFlushAsync(packet);
    }
}