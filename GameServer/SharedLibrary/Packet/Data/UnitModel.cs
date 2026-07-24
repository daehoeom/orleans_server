using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class UnitModel
    {
        [Key(0)]
        public int UnitId { get; set; }
        
        [Key(1)]
        public int Stack { get; set; }
    }   
}

