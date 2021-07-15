using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(16, Version.NotApplicable, BanchoPacketType.Out)]
    public class SendFrameBundle : IGenericPacketOut
    {
        [BanchoSerializable] public byte[] Frames;
    }
}