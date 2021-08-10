namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class ChannelLeft : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.ChannelLeft {ChannelName = ChannelName};
    }
}