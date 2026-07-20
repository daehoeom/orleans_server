using System;

namespace SharedLibrary.Packet.Base
{
    public sealed record StreamPacket
    {
        public PacketHeaderType HeaderType { get; set; }
        public ReadOnlyMemory<byte> Body { get; set; }
    }
}