using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(2, Version.B904, BanchoPacketType.Out)]
    public struct UserQuit : ISharedPacketOut
    {
        [BanchoSerializable] public int UserID;
    }
}