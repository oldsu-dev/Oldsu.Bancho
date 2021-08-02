using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(38, Version.B904, BanchoPacketType.Out)]
    public struct MatchJoinFail : IB904PacketOut { }
}