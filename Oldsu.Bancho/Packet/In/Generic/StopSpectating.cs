using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(18, Version.NotApplicable, BanchoPacketType.In)]
    public struct StopSpectating : IntoPacket<Shared.In.StopSpectating>
    {
        public Shared.In.StopSpectating IntoPacket()
        {
            return new Shared.In.StopSpectating { };
        }
    }
}