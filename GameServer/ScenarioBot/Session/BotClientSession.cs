using System.Collections.Concurrent;
using DotNetty.Transport.Channels;
using MessagePack;
using SharedLibrary;
using SharedLibrary.Packet;
using SharedLibrary.Packet.Base;

namespace ScenarioBot.Session;

public class BotClientSession(long botId)
{
    public long BotId { get; } = botId;
    public IChannel Channel { get; private set; } = null!;

    private readonly ConcurrentDictionary<PacketHeaderType, TaskCompletionSource<StreamPacket>> _pending = new();

    internal void Attack(IChannel channel) => Channel = channel;

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
        var headerType = HeaderCache<TResponse>.HeaderType;
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