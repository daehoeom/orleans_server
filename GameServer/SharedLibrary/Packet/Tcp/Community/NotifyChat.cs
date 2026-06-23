using MessagePack;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class ChatNtf 
    {
        [Key(0)]
        public string Message { get; set; }
    }
}

