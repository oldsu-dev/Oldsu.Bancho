namespace Oldsu.Bancho.Packet.Out.B394a
{
    [BanchoPacket(5)]
    public class Login : IB394APacketOut
    {
        [BanchoSerializable] 
        public int LoginStatus;

        [BanchoSerializable] 
        public byte Privilege;
    }
}