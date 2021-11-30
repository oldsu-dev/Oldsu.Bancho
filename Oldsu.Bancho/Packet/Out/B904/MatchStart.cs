using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(47, Version.B904, BanchoPacketType.Out)]
    public class MatchStart : IB904PacketOut
    {
        [BanchoSerializable()] public MatchState MatchState;
    }
}