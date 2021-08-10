namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct AutojoinChannelAvailable : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.AutojoinChannelAvailable
            {ChannelName = ChannelName};
    }
}