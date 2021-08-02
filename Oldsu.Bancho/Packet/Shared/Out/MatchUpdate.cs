using Oldsu.Bancho.Multiplayer;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchUpdate : ISharedPacketOut, Into<IB904PacketOut>
    {
        public Match Match { get; set; }

        public IB904PacketOut Into() => new Packet.Out.B904.MatchUpdate {Match = Match.ToB904Match()};
    }
}