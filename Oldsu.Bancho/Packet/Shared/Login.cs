namespace Oldsu.Bancho.Packet.Shared
{
    public struct Login : ISharedPacket, Into<IB394APacketOut>
    {
        public int LoginStatus { get; set; }
        public byte Privilege { get; set; }

        public IB394APacketOut Into()
        {
            var packet = new Out.B394a.Login();
            
            packet.LoginStatus = LoginStatus;
            packet.Privilege = Privilege;

            return packet;
        }
    }
}