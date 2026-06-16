using MessagePack;

namespace SharedLibrary.Packet
{
    [MessagePackObject]
    public class BaseRequestPacket : IPacket
    {
    }

    [MessagePackObject]
    public class BaseResponsePacket : IPacket
    {
    }

    [MessagePackObject]
    public class BaseNtfPacket : IPacket
    {
    }   
}

