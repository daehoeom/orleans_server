using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Attendance
{
    [MessagePackObject]
    public class CheckAttendanceReq
    {
        [Key(0)]
        public int EventId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.CheckAttendance)]
    public class CheckAttendanceRes
    {
        [Key(0)]
        public int EventId { get; set; }

        [Key(1)]
        public int Day { get; set; }

        [Key(2)]
        public bool Claimed { get; set; }
    }
}
