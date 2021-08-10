using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(62, Version.B904, BanchoPacketType.Out)]
    public struct MatchSkip : IB904PacketOut { }
}