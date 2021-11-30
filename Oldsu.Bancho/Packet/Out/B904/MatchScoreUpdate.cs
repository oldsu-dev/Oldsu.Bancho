using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(49, Version.B904, BanchoPacketType.Out)]
    public struct MatchScoreUpdate : IB904PacketOut
    {
        [BanchoSerializable()] public ScoreFrame ScoreFrame;
    }
}