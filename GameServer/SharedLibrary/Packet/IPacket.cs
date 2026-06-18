namespace SharedLibrary.Packet
{
    public interface IRequestPacket
    {
        public PacketHeaderType HeaderType { get; set; }
    }

    public interface IResponsePacket
    {
        public PacketHeaderType HeaderType { get; set; }
        public ResultCode ResultCode { get; set; }
    }

    public interface INtfPacket
    {
        public PacketHeaderType HeaderType { get; set; }
    }    
}

