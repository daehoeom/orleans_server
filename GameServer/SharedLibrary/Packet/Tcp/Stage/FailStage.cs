using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Stage
{
    [MessagePackObject]
    public class FailStageReq
    {
        [Key(0)]
        public int StageId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.FailStage)]
    public class FailStageRes
    {
        [Key(0)]
        public int StageId { get; set; }
    }
}
