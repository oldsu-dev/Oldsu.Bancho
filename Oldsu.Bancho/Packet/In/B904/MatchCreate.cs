using Oldsu.Bancho.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(32, Version.B904, BanchoPacketType.In)]
    public class MatchCreate
    {
        [BanchoSerializable] private Match Match { get; set; }
    }
}