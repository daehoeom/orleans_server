using MessagePack;

namespace SharedLibrary.Packet.Response
{
    [MessagePackObject]
    public class ShopInfoRes : BaseResponsePacket
    {
        [Key(0)]
        public ResultCode ResultCode { get; set; }
    }
}

