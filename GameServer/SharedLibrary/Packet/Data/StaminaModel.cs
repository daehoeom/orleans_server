using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class StaminaModel
    {
        [Key(0)]
        public int Amount { get; set; }
        
        [Key(1)]
        public int MaxAmount { get; set; }
    }
}

