using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Objects.B904;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchStart : ISharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public MatchState MatchState { get; set; }
        
        public IB904PacketOut IntoPacket() => new Packet.Out.B904.MatchStart 
            {Match = Match.FromMatchState(MatchState)};
    }
}