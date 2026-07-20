using MessagePack;

namespace SharedLibrary.Packet.Base
{
    [MessagePackObject]
    public class BaseRequestPacket : IRequestPacket
    {
        [Key(0)]
        public PacketHeaderType HeaderType { get; set; }
    }

    [MessagePackObject]
    public class BaseResponsePacket<T> : IResponsePacket
        where T : class, new()
    {
        [Key(1)]
        public PacketHeaderType HeaderType { get; set; }

        [Key(2)]
        public ResultCode ResultCode { get; set; }
        
        [Key(3)]
        public T Stream { get; init; } = default!;
    }

    [MessagePackObject]
    public class BaseNtfPacket<T> : INtfPacket
    {
        [Key(0)]
        public PacketHeaderType HeaderType { get; set; }
        
        [Key(1)]
        public T Stream { get; init; } = default!;
    }   
}

