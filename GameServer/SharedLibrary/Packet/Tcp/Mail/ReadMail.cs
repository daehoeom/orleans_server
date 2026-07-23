using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Mail
{
    [MessagePackObject]
    public class ReadMailReq
    {
        [Key(0)]
        public long Id { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.ReadMail)]
    public class ReadMailRes
    {
        [Key(0)]
        public MailInfo MailInfo { get; set; } = new();

        [Key(1)]
        public List<WalletInfo> WalletInfo { get; set; } = new();
    }
}
