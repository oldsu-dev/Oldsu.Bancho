namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Ping : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.Ping();
    }
}