using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class ItemInfo
    {
        [Key(0)]
        public int ItemId { get; set; }
        
        [Key(1)]
        public int Count { get; set; }
    }
}