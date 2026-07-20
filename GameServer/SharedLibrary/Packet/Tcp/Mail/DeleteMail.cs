using MessagePack;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet.Tcp.Mail
{
    [MessagePackObject]
    public class DeleteMailReq
    {
        [Key(0)]
        public long Id { get; set; }
    }

    [MessagePackObject]
    [Response(PacketHeaderType.DeleteMail)]
    public class DeleteMailRes
    {
        [Key(0)]
        public long Id { get; set; }
    }
}
