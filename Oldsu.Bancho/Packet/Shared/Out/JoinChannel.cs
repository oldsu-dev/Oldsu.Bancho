namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct JoinChannel : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ChannelName { get; init; }

        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.JoinChannel
            {
                ChannelName = ChannelName
            };

            return packet;
        }
    }
}