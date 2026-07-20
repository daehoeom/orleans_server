using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Attendance
{
    [MessagePackObject]
    public class ClaimAttendanceReq
    {
        [Key(0)]
        public int EventId { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.ClaimAttendance)]
    public class ClaimAttendanceRes
    {
        [Key(0)]
        public int EventId { get; set; }

        [Key(1)]
        public int Day { get; set; }

        [Key(2)]
        public CurrencyType RewardCurrencyType { get; set; }

        [Key(3)]
        public long RewardCurrencyAmount { get; set; }

        [Key(4)]
        public int RewardItemId { get; set; }

        [Key(5)]
        public int RewardItemCount { get; set; }

        [Key(6)]
        public List<WalletInfo> WalletInfo { get; set; } = new();
    }
}
