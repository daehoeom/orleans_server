namespace SharedLibrary.Packet.Base;

public sealed record StreamPacket
{
    public PacketHeaderType HeaderType { get; init; }
    public ReadOnlyMemory<byte> Body { get; init; }
}