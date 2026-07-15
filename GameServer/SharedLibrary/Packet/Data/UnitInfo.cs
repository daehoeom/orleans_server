using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class UnitInfo
    {
        [Key(0)]
        public int UnitId { get; set; }
        
        [Key(1)]
        public int Stack { get; set; }
    }   
}

