namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchJoinFail : ISharedPacketOut, Into<IB904PacketOut>
    {
        public IB904PacketOut Into() => new Packet.Out.B904.MatchJoinFail();
    }
}