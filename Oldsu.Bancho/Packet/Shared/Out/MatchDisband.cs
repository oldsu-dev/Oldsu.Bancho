namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct MatchDisband : ISharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public int MatchID { get; set; }
        
        public IB904PacketOut IntoPacket() => new Packet.Out.B904.MatchDisband {MatchID = MatchID};
    }
}