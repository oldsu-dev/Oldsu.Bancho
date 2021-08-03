namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Ping : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public IGenericPacketOut Into() => new Packet.Out.Generic.Ping();
    }
}