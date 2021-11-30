namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class ChannelAvailable : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.ChannelAvailable {ChannelName = ChannelName};
    }
}