namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class ChannelJoined : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; init; }

        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.ChannelJoined()
            {
                ChannelName = ChannelName
            };

            return packet;
        }
    }
}