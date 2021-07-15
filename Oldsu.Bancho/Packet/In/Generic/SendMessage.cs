using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(1, Version.NotApplicable, BanchoPacketType.In)]
    public struct SendMessage : Into<Shared.In.SendMessage>, IGenericPacketIn
    {
        public Shared.In.SendMessage Into() => new();
    }
}