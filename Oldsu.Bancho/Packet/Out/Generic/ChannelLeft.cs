using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(67, Version.NotApplicable, BanchoPacketType.Out)]
    public struct ChannelLeft : IGenericPacketOut
    {
        [BanchoSerializable] public string ChannelName;
    }
}