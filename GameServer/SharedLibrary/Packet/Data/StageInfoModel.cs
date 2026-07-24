using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class StageInfoModel
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
        public int ClearScore { get; set; }
    }   
}

