namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct ChannelJoined : ISharedPacketOut, IntoPacket<IGenericPacketOut>
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