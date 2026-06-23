using DotNetty.Transport.Channels;

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


    public void Dispose() => _authTimeoutCts.Dispose();
}