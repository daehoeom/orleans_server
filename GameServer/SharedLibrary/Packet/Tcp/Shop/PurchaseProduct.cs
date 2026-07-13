using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Shop
{
    [MessagePackObject]
    public class PurchaseProductReq
    {
        [Key(0)]
        public int ProductId { get; set; }

        [Key(1)]
        public int Count { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.Purchase)]
    public class PurchaseProductRes
    {
        [Key(0)]
        public int ProductId { get; set; }
        
        [Key(1)]
        public int RemainAmount { get; set; }
    }
}

