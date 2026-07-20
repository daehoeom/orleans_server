using System;
using System.Collections.Generic;
using MessagePack;

namespace SharedLibrary.Packet.Data
{
    [MessagePackObject]
    public class MailInfo
    {
        [Key(0)]
        public long Id { get; set; }

        [Key(1)]
        public long MailId { get; set; }

        [Key(2)]
        public MailType Type { get; set; }

        [Key(3)]
        public string Title { get; set; } = string.Empty;

        [Key(4)]
        public string Body { get; set; } = string.Empty;

        [Key(5)]
        public List<MailRewardEntry> Rewards { get; set; } = new();

        [Key(6)]
        public bool IsRead { get; set; }

        [Key(7)]
        public DateTime ExpiredAt { get; set; }
    }
}
