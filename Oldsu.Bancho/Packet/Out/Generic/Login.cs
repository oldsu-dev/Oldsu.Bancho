using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(5, Version.NotApplicable, BanchoPacketType.Out)]
    public class Login : IGenericPacketOut
    {
        [BanchoSerializable] public int LoginStatus;
    }
}