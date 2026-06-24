using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class SendChatReq
    {
        [Key(0)]
        public ChatType ChatType { get; set; }
        
        [Key(1)]
        public long TargetUser { get; set; }
        
        [Key(2)]
        public string Message { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.SendChat)]
    public class SendChatRes
    {
        [Key(0)]
        public ChatType ChatType { get; set; }
        
        [Key(1)]
        public long TargetUser { get; set; }
        
        [Key(2)]
        public string Message { get; set; }
    }
}

