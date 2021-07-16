using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(72, Version.B904, BanchoPacketType.Out)]
    public struct BanchoPrivileges : IB904PacketOut
    {
        [BanchoSerializable] public int Privileges;
    }
}