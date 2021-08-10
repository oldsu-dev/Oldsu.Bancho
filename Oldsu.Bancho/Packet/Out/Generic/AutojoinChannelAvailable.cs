using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(68, Version.NotApplicable, BanchoPacketType.Out)]
    public struct AutojoinChannelAvailable : IGenericPacketOut
    {
        [BanchoSerializable] public string ChannelName;
    }
}