namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class AutojoinChannelAvailable : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.AutojoinChannelAvailable
            {ChannelName = ChannelName};
    }
}