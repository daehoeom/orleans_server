using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class SendChatReq
    {
        [Key(0)]
        public string Message { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.SendChat)]
    public class SendChatRes
    {
        
    }
}

