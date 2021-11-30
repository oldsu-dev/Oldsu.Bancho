namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class FellowSpectatorJoined : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int UserID { get; set; }
        
        public IGenericPacketOut IntoPacket() => 
            new Packet.Out.Generic.FellowSpectatorJoined {UserID = UserID};
    }
}