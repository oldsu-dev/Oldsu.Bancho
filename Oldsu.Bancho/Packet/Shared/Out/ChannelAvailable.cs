namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct ChannelAvailable : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.ChannelAvailable {ChannelName = ChannelName};
    }
}