using System;

namespace SharedLibrary.Packet.Base
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute : Attribute
    {
        public PacketHeaderType HeaderType { get; }

        public PacketHandlerAttribute(PacketHeaderType headerType) => HeaderType = headerType;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ResponseAttribute : Attribute
    {
        public PacketHeaderType HeaderType { get; }

        public ResponseAttribute(PacketHeaderType headerType)
            => HeaderType = headerType;
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NotifyAttribute : Attribute
    {
        public PacketHeaderType HeaderType { get; }

        public NotifyAttribute(PacketHeaderType headerType)
            => HeaderType = headerType;
    }
}

