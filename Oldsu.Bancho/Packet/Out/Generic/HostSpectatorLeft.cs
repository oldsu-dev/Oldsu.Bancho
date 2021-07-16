using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(44, Version.NotApplicable, BanchoPacketType.Out)]
    public struct HostSpectatorLeft : IGenericPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}