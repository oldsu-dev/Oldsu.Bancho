using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(4, Version.NotApplicable, BanchoPacketType.In)]
    public struct Pong : IntoPacket<Shared.In.Pong>
    {
        public Shared.In.Pong IntoPacket() => new();
    }
}