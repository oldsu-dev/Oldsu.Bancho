using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(64, Version.NotApplicable, BanchoPacketType.Out)]
    public struct JoinChannel : IGenericPacketOut
    {
        [BanchoSerializable] public string ChannelName;
    }
}