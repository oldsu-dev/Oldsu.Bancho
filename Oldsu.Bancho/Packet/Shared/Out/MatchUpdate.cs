using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Bancho.Packet.Objects.B904;
using MatchState = Oldsu.Bancho.Packet.Objects.B904.MatchState;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchUpdate : SharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public Match Match { get; set; }

        public IB904PacketOut IntoPacket() =>
            new Packet.Out.B904.MatchUpdate {MatchState = MatchState.FromMatchState(Match)};
    }
}