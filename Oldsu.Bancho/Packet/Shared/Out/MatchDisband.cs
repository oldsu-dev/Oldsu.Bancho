namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchDisband : ISharedPacketOut, Into<IB904PacketOut>
    {
        public int MatchID { get; set; }
        
        public IB904PacketOut Into() => new Packet.Out.B904.MatchDisband {MatchID = MatchID};
    }
}