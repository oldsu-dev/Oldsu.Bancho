using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(2, Version.NotApplicable, BanchoPacketType.Out)]
    public struct UserQuit : IGenericPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}