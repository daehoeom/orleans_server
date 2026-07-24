using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class ShopProductModel
    {
        [Key(0)]
        public int ProductId { get; set; }
        
        [Key(1)]
        public int PurchaseCount { get; set; }
    }
}