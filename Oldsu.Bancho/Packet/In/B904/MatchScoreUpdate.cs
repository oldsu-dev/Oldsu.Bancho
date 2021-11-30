using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(48, Version.B904, BanchoPacketType.In)]
    public struct MatchScoreUpdate : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable()] public ScoreFrame ScoreFrame;

        public ISharedPacketIn IntoPacket() => new Shared.In.MatchScoreUpdate
            {ScoreFrame = Bancho.Objects.ScoreFrame.FromB904ScoreFrame(ScoreFrame)};
    }
}