using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B394a
{
    [BanchoPacket(5, Version.B394A)]
    public class Login : IB394APacketOut
    {
        [BanchoSerializable] public int LoginStatus;
        [BanchoSerializable] public byte Privilege;
    }
}