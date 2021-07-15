using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(19, Version.NotApplicable, BanchoPacketType.In)]
    public class FrameBundle : BanchoBuffer, Into<Shared.In.FrameBundle>, IGenericPacketIn
    {
        public Shared.In.FrameBundle Into()
        {
            return new Shared.In.FrameBundle
            {
                Frames = Data
            };
        }
    }
}