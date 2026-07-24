using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Stage
{
    [MessagePackObject]
    public class ClearStageReq
    {
        [Key(0)]
        public int StageId { get; set; }

        [Key(1)]
        public bool Mission1 { get; set; }

        [Key(2)]
        public bool Mission2 { get; set; }

        [Key(3)]
        public bool Mission3 { get; set; }

        [Key(4)]
        public short ClearScore { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.ClearStage)]
    public class ClearStageRes
    {
        [Key(0)]
        public StageInfoModel StageInfoModel { get; set; } = new();

        [Key(1)]
        public List<WalletModel> WalletInfo { get; set; } = new();

        [Key(2)]
        public int Level { get; set; }

        [Key(3)]
        public long Exp { get; set; }
    }
}
