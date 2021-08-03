namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct MatchDisband : ISharedPacketOut, Into<IB904PacketOut>
    {
        public int MatchID { get; set; }
        
        public IB904PacketOut Into() => new Packet.Out.B904.MatchDisband {MatchID = MatchID};
    }
}