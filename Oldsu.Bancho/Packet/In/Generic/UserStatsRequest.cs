using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(3, Version.NotApplicable, BanchoPacketType.In)]
    public struct UserStatsRequest : IntoPacket<Shared.In.UserStatsRequest>
    {
        public Shared.In.UserStatsRequest IntoPacket() => new();
    }
}