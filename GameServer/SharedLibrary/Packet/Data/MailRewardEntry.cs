using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class MailRewardEntry
    {
        [Key(0)]
        public CurrencyType CurrencyType { get; set; }

        [Key(1)]
        public long CurrencyAmount { get; set; }

        [Key(2)]
        public int ItemId { get; set; }

        [Key(3)]
        public int ItemCount { get; set; }
    }
}
