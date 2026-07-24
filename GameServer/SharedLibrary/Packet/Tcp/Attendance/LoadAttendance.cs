using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Attendance
{
    [MessagePackObject]
    public class LoadAttendanceReq
    {
        [Key(0)]
        public int EventId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.LoadAttendance)]
    public class LoadAttendanceRes
    {
        [Key(0)]
        public int EventId { get; set; }

        [Key(1)]
        public int Day { get; set; }

        [Key(2)] 
        public Dictionary<int, bool> ReceiveRewardFlag { get; set; } = new();
    }
}
