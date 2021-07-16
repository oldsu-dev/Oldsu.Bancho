using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(18, Version.NotApplicable, BanchoPacketType.In)]
    public struct StopSpectating : Into<Shared.In.StopSpectating>, IGenericPacketIn
    {
        public Shared.In.StopSpectating Into()
        {
            return new Shared.In.StopSpectating { };
        }
    }
}