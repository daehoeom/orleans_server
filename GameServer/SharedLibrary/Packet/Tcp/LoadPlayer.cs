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
        public List<WalletInfo> WalletInfo { get; set; } = new List<WalletInfo>();
        
        [Key(2)]
        public List<UnitInfo> UnitInfo { get; set; } = new List<UnitInfo>();
        
        [Key(3)]
        public List<StageInfo> StageInfo { get; set; } = new List<StageInfo>();
        
        [Key(4)]
        public StaminaInfo StaminaInfo { get; set; } = new StaminaInfo();
        
        [Key(5)]
        public List<ItemInfo> ItemInfo { get; set; } = new List<ItemInfo>();
    }
}

