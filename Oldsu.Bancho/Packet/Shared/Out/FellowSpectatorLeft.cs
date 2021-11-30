namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class FellowSpectatorLeft : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int UserID { get; set; }
        public IGenericPacketOut IntoPacket() => 
            new Packet.Out.Generic.FellowSpectatorLeft() {UserID = UserID};
    }
}