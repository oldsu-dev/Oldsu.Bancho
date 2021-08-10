using System.Linq;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(28, Version.B904, BanchoPacketType.Out)]
    public struct MatchUpdate : IB904PacketOut
    {
        [BanchoSerializable] public Match Match;
    }
}