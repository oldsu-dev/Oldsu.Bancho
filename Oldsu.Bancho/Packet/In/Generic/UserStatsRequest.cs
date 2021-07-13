using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(3, Version.NotApplicable, BanchoPacketType.In)]
    public struct UserStatsRequest : Into<Shared.In.UserStatsRequest>, IGenericPacketIn
    {
        public Shared.In.UserStatsRequest Into() => new();
    }
}