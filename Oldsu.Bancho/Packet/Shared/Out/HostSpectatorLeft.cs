namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct HostSpectatorLeft : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public int UserID { get; init; }

        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.HostSpectatorLeft
            {
                UserID = UserID,
            };

            return packet;
        }
    }
}