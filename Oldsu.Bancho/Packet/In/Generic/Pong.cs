using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(4, Version.NotApplicable, BanchoPacketType.In)]
    public struct Pong : Into<Shared.In.Pong>, IGenericPacketIn
    {
        public Shared.In.Pong Into() => new();
    }
}