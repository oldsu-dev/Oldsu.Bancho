using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(15, Version.NotApplicable, BanchoPacketType.Out)]
    public struct FellowSpectatorLeft : IGenericPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}