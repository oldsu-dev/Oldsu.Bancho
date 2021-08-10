using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(59, Version.B904, BanchoPacketType.Out)]
    public struct MatchComplete : IB904PacketOut
    { }
}