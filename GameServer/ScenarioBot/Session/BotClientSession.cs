using System.Collections.Concurrent;
using DotNetty.Transport.Channels;
using MessagePack;
using SharedLibrary;
using SharedLibrary.Packet;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Tcp;

namespace ScenarioBot.Session;

public class BotClientSession
{
    public long BotId { get; }
    public IChannel Channel { get; private set; } = null!;

    private readonly ConcurrentDictionary<PacketHeaderType, TaskCompletionSource<StreamPacket>> _pending = new();
    private readonly ConcurrentDictionary<PacketHeaderType, Func<StreamPacket, Task>> _notifyHandlers = new();

    internal void Attach(IChannel channel) => Channel = channel;

    public BotClientSession(long botId)
    {
        BotId = botId;
        
        RegisterNtf();
    }

    private void RegisterNtf()
    {
        OnNotify<ChatNtf>(ntf => Task.CompletedTask);
    }
    
    public void OnNotify<T>(Func<T, Task> handler)
        where T : class, new()
    {
        var headerType = NotifyHeaderCache<T>.HeaderType;
        _notifyHandlers[headerType] = async stream =>
        {
            var packet = MessagePackSerializer.Deserialize<BaseNtfPacket<T>>(stream.Body.ToArray());
            await handler(packet.Stream);
        };
    }
    
    public async Task SendAsync<TRequest>(PacketHeaderType headerType, TRequest request)
    {
        var body = MessagePackSerializer.Serialize(request);

        // [4B length][4B HeaderType][body]
        var buffer = Channel.Allocator.Buffer(8 + body.Length);
        buffer.WriteInt(4 + body.Length);   // length = HeaderType(4) + body
        buffer.WriteInt((int)headerType);
        buffer.WriteBytes(body);

        await Channel.WriteAndFlushAsync(buffer);
    }

    public async Task<BaseResponsePacket<TResponse>> WaitForResponseAsync<TResponse>(
        TimeSpan? timeout = null) where TResponse : class, new()
    {
        var headerType = ResponseHeaderCache<TResponse>.HeaderType;
        var tcs = new TaskCompletionSource<StreamPacket>(TaskCreationOptions.RunContinuationsAsynchronously);

        _pending[headerType] = tcs;

        try
        {
            var stream = await tcs.Task.WaitAsync(timeout ?? TimeSpan.FromSeconds(5));
            return MessagePackSerializer.Deserialize<BaseResponsePacket<TResponse>>(stream.Body);
        }
        finally
        {
            _pending.TryRemove(headerType, out _);
        }
    }

    internal void OnReceive(StreamPacket stream)
    {
        if (_pending.TryGetValue(stream.HeaderType, out var tcs))
        {
            tcs.TrySetResult(stream);
            return;
        }

        if (_notifyHandlers.TryGetValue(stream.HeaderType, out var handler))
        {
            _ = handler(stream);
        }
    }

    internal void OnDisconnected()
    {
        foreach (var tcs in _pending.Values)
        {
            tcs.TrySetCanceled();
        }
        
        _pending.Clear();
    }
}