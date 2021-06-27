namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Login : ISharedPacket, Into<IB394APacketOut>
    {
        public int LoginStatus { get; init; }
        public byte Privilege { get; init; }

        public IB394APacketOut Into()
        {
            var packet = new Packet.Out.B394a.Login();
            
            packet.LoginStatus = LoginStatus;
            packet.Privilege = Privilege;

            return packet;
        }
    }
}