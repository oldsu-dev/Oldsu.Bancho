using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(29, Version.B904, BanchoPacketType.Out)]
    public class MatchDisband : IB904PacketOut
    {
        [BanchoSerializable] public int MatchID;
    }
}