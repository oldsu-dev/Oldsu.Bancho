using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(65, Version.NotApplicable, BanchoPacketType.Out)]
    public struct ChannelJoined : IGenericPacketOut
    {
        [BanchoSerializable] public string ChannelName;
    }
}