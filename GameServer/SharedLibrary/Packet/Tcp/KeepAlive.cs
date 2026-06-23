using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class KeepAliveReq
    {
    }

    [MessagePackObject]
    [Response(PacketHeaderType.KeepAlive)]
    public class KeepAliveRes
    {
        [Key(0)]
        public long DateTimeTicks { get; set; }
    }    
}