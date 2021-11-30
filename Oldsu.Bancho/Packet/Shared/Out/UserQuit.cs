namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class UserQuit : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int UserID { get; init; }
        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.UserQuit
            {
                UserID = UserID
            };

            return packet;
        }
    }
}