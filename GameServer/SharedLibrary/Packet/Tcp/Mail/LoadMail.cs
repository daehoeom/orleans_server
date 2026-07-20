using System.Collections.Generic;
using MessagePack;
using SharedLibrary.Packet.Base;
using SharedLibrary.Packet.Data;

namespace SharedLibrary.Packet.Tcp.Mail
{
    [MessagePackObject]
    public class LoadMailReq
    {
    }

    [MessagePackObject]
    [Response(PacketHeaderType.LoadMail)]
    public class LoadMailRes
    {
        [Key(0)]
        public List<MailInfo> Mails { get; set; } = new();
    }
}
