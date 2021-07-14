namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class Ping : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public IGenericPacketOut Into() => new Packet.Out.Generic.Ping();
    }
}