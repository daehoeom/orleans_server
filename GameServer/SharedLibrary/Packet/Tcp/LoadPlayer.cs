using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class LoadPlayerReq
    {
    }

    [MessagePackObject]
    [Response(PacketHeaderType.LoadPlayer)]
    public class LoadPlayerRes
    {
        [Key(0)]
        public long PlayerId { get; set; }
    }
}

