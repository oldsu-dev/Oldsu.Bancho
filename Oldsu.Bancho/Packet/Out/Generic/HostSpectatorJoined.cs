using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(43, Version.NotApplicable, BanchoPacketType.Out)]
    public struct HostSpectatorJoined : IGenericPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}