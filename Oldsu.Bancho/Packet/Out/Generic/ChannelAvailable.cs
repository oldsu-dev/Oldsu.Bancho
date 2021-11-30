using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(66, Version.NotApplicable, BanchoPacketType.Out)]
    public struct ChannelAvailable : IGenericPacketOut
    {
        [BanchoSerializable] public string ChannelName;
    }
}