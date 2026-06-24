using MessagePack;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class AuthPlayerReq
    {
        [Key(0)]
        public long AccountId { get; set; }
        
        [Key(1)]
        public string AccessToken { get; set; } = string.Empty;
    }

    public class AuthPlayerRes
    {
        [Key(0)]
        public long PlayerId { get; set; }
    }
}

