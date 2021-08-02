using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(27, Version.B904, BanchoPacketType.Out)]
    public struct MatchUpdate : IB904PacketOut
    {
        [BanchoSerializable] public Match Match;
    }
}