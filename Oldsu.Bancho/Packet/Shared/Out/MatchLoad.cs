namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchLoad : SharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public IB904PacketOut IntoPacket() => new Packet.Out.B904.MatchLoad();
    }
}