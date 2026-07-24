using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp
{
    [MessagePackObject]
    public class LoadPlayerReq
    {
    }

    [MessagePackObject]
    [Response(PacketHeaderType.LoadPlayer)]
    public class LoadPlayerRes
    {
        [Key(0)]
        public long PlayerId { get; set; }

        [Key(1)] 
        public List<WalletModel> WalletInfo { get; set; } = new List<WalletModel>();
        
        [Key(2)]
        public List<UnitModel> UnitInfo { get; set; } = new List<UnitModel>();
        
        [Key(3)]
        public List<StageInfoModel> StageInfo { get; set; } = new List<StageInfoModel>();
        
        [Key(4)]
        public StaminaModel StaminaModel { get; set; } = new StaminaModel();
        
        [Key(5)]
        public List<ItemModel> ItemInfo { get; set; } = new List<ItemModel>();
    }
}

