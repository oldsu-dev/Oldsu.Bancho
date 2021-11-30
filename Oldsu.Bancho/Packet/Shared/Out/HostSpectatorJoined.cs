namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class HostSpectatorJoined : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int UserID { get; init; }

        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.HostSpectatorJoined
            {
                UserID = UserID,
            };

            return packet;
        }
    }
}