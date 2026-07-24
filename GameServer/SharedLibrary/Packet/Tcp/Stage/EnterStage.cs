using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Stage
{
    [MessagePackObject]
    public class EnterStageReq
    {
        [Key(0)]
        public int StageId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.EnterStage)]
    public class EnterStageRes
    {
        [Key(0)]
        public int StageId { get; set; }

        [Key(1)]
        public StaminaModel StaminaModel { get; set; } = new();
    }
}
