using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class WalletModel
    {
        [Key(0)]
        public CurrencyType CurrencyType { get; set; }
        
        [Key(1)]
        public long Amount { get; set; }
    }
}

