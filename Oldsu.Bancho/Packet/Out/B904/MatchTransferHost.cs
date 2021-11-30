using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(51, Version.B904, BanchoPacketType.Out)]
    public struct MatchTransferHost : IB904PacketOut
    { }
}