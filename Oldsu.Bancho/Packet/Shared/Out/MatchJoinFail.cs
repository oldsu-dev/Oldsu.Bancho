namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct MatchJoinFail : ISharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public IB904PacketOut IntoPacket() => new Packet.Out.B904.MatchJoinFail();
    }
}