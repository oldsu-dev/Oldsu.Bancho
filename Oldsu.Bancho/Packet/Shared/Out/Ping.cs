namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class Ping : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.Ping();
    }
}