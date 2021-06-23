namespace Oldsu.Bancho.Packet.Shared
{
    public class Login : ISharedPacket, Into<IB394APacketOut>
    {
        public int LoginStatus;
        public byte Privilege;
        
        public Login(params object[] fields)
        {
            LoginStatus = (int)fields[0];
            Privilege = (byte)fields[1];
        }

        public IB394APacketOut Into()
        {
            var packet = new Out.B394a.Login();
            
            packet.LoginStatus = LoginStatus;
            packet.Privilege = Privilege;

            return packet;
        }
    }
}