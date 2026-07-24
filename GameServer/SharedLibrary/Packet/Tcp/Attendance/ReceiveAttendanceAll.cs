using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Attendance
{
    [MessagePackObject]
    public class ReceiveAttendanceRewardAllReq
    {
        [Key(0)]
        public int EventId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.ReceiveAttendanceRewardAll)]
    public class ReceiveAttendanceRewardAllRes
    {
        [Key(0)]
        public int EventId { get; set; }
        
        [Key(1)]
        public RewardResultModel RewardResult { get; set; } = new();
    }
}