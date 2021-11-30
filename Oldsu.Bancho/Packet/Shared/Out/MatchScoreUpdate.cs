using Oldsu.Bancho.Objects;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchScoreUpdate : SharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public ScoreFrame ScoreFrame { get; set; }

        public IB904PacketOut IntoPacket() => new Packet.Out.B904.MatchScoreUpdate
        {
            ScoreFrame = Objects.B904.ScoreFrame.FromSharedScoreFrame(ScoreFrame)
        };
    }
}