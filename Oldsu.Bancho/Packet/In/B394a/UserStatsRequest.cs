using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B394a
{
    [BanchoPacket(3, Version.B394A, BanchoPacketType.In)]
    public struct UserStatsRequest : Into<Shared.In.UserStatsRequest>
    {
        public Shared.In.UserStatsRequest Into() => new();
    }
}