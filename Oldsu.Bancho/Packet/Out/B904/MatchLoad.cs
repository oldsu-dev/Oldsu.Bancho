using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(54, Version.B904, BanchoPacketType.Out)]
    public struct MatchLoad : IB904PacketOut { }
}