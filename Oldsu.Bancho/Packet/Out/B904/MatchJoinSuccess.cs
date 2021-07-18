using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(37, Version.B904, BanchoPacketType.Out)]

    public class MatchJoinSuccess : IB904PacketOut
    {
        [BanchoSerializable] public Match Match;
    }
}