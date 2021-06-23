namespace Oldsu.Bancho.Packet.Out.B394a
{
    public struct Login : IB394APacketOut
    {
        [BanchoSerializable] 
        public int LoginStatus;

        [BanchoSerializable] 
        public byte Privilege;
    }
}