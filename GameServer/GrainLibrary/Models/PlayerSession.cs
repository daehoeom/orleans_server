using DotNetty.Transport.Channels;
using SharedLibrary;
using SharedLibrary.Packet;
using SharedLibrary.Packet.Base;

namespace ServerLibrary.Models;

public class PlayerSession : IDisposable
{
    private long _sessionId;

    private readonly CancellationTokenSource _authTimeoutCts = new();
    
    public IChannelHandlerContext Channel { get; init; }
    public long SessionId => _sessionId;

    public PlayerSession CreateSession(long sessionId)
    {
        _sessionId = sessionId;
        return this;
    }

    public void StartAuthTimeout(TimeSpan timeout)
    {
        _ = Task.Delay(timeout, _authTimeoutCts.Token)
            .ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    return;
                }

                Channel.CloseAsync();
            });
    }

    public async Task SendAsync<T>(T body, ResultCode resultCode = ResultCode.Success)
        where T : class, new()
    {
        var packet = new BaseResponsePacket<T>
        {
            HeaderType = ResponseHeaderCache<T>.HeaderType,
            Stream = body,
            ResultCode = resultCode,
        };

        await WriteAsync(packet);
    }

    public Task NotifyAsync<T>(T ntf)
        where T : class, new()
    {
        var packet = new BaseNtfPacket<T>
        {
            HeaderType = NotifyHeaderCache<T>.HeaderType,
            Stream = ntf,
        };

        _ = WriteAsync(packet);
        return Task.CompletedTask;
    }
    
    private async Task WriteAsync<T>(T packet)
    {
        await Channel.WriteAndFlushAsync(packet);
    } 
    
    public void Dispose() => _authTimeoutCts.Dispose();
}