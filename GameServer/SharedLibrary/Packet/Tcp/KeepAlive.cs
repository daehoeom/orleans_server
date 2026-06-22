using MessagePack;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class KeepAliveReq
    {
    }

    [MessagePackObject]
    public class KeepAliveRes
    {
        [Key(0)]
        public long DateTimeTicks { get; set; }
    }    
}