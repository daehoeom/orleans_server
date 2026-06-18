namespace SharedLibrary.Packet.Base
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketPacketHandlerAttribute : Attribute
    {
        public PacketHeaderType HeaderType { get; }

        public PacketPacketHandlerAttribute(PacketHeaderType headerType) => HeaderType = headerType;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ResponseAttribute : Attribute
    {
        public PacketHeaderType HeaderType { get; }

        public ResponseAttribute(PacketHeaderType headerType)
            => HeaderType = headerType;
    }
}

