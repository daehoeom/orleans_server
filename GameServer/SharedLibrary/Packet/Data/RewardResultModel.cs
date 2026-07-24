using System.Collections.Generic;
using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class RewardResultModel
    {
        [Key(0)] 
        public List<WalletModel> RewardCurrencies { get; set; } = new();

        [Key(1)]
        public List<ItemModel> RewardItems { get; set; } = new();
    }
}