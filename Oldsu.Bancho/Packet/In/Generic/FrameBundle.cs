using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(19, Version.NotApplicable, BanchoPacketType.In)]
    public class FrameBundle : BanchoBuffer, IntoPacket<Shared.In.FrameBundle>
    {
        public Shared.In.FrameBundle IntoPacket()
        {
            return new Shared.In.FrameBundle
            {
                Frames = Data
            };
        }
    }
}