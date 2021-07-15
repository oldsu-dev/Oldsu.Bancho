namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct JoinChannel : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public string ChannelName { get; init; }

        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.JoinChannel
            {
                ChannelName = ChannelName
            };

            return packet;
        }
    }
}