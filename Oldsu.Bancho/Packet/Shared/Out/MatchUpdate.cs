using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Objects.B904;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct MatchUpdate : ISharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public MatchState MatchState { get; set; }

        public IB904PacketOut IntoPacket() =>
            new Packet.Out.B904.MatchUpdate {Match = Match.FromMatchState(MatchState)};
    }
}