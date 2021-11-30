namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class HostSpectatorLeft : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int UserID { get; init; }

        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.HostSpectatorLeft
            {
                UserID = UserID,
            };

            return packet;
        }
    }
}