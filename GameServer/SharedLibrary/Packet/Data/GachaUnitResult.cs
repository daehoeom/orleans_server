using MessagePack;

namespace SharedLibrary.Packet.Data
{
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
}

