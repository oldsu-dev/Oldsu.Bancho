namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class HostSpectatorJoined : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public int UserID { get; init; }

        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.HostSpectatorJoined
            {
                UserID = UserID,
            };

            return packet;
        }
    }
}