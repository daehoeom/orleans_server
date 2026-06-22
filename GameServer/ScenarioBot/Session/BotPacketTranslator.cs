using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using SharedLibrary;
using SharedLibrary.Packet.Base;

namespace ScenarioBot.Session;

public class BotPacketDecoder : MessageToMessageDecoder<IByteBuffer>
{
    protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
    {
        var headerType = (PacketHeaderType)message.ReadInt();

        var bodyLen = message.ReadableBytes;
        var body = new byte[bodyLen];
        message.ReadBytes(body);
        
        output.Add(new StreamPacket
        {
            HeaderType = headerType,
            Body = body.AsMemory()
        });
    }
}

public class BotPacketEncoder : MessageToByteEncoder<object>
{
    protected override void Encode(IChannelHandlerContext context, object message, IByteBuffer output)
    {
        var body = MessagePackSerializer.Serialize(message.GetType(), message);

        output.WriteInt(body.Length);
        output.WriteBytes(body);
    }
}