using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(19, Version.NotApplicable, BanchoPacketType.In)]
    public struct FrameBundle : Into<Shared.In.FrameBundle>, IGenericPacketIn
    {
        [BanchoSerializable] public byte[] Frames;
        
        public Shared.In.FrameBundle Into()
        {
            return new Shared.In.FrameBundle
            {
                Frames = Frames
            };
        }
    }
}