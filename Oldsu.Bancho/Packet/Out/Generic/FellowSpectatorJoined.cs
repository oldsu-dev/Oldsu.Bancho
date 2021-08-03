using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(14, Version.NotApplicable, BanchoPacketType.Out)]
    public struct FellowSpectatorJoined : IGenericPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}
