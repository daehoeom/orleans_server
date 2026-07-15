using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Gacha
{
    [MessagePackObject]
    public class RollGachaReq
    {
        [Key(0)]
        public int GachaId { get; set; }

        [Key(1)]
        public int Count { get; set; }
    }

    [MessagePackObject]
    public class GachaUnitResult
    {
        [Key(0)]
        public int UnitId { get; set; }

        [Key(1)]
        public int Level { get; set; }

        [Key(2)]
        public int Stack { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.RollGacha)]
    public class RollGachaRes
    {
        [Key(0)]
        public int GachaId { get; set; }

        [Key(1)]
        public List<GachaUnitResult> Units { get; set; } = new();
    }
}
