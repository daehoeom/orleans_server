using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    [Notify(PacketHeaderType.ChatNtf)]
    public class ChatNtf 
    {
        [Key(0)]
        public string Message { get; set; }
    }
}

