namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Login : ISharedPacketOut, Into<IB394APacketOut>
    {
        public int LoginStatus { get; init; }
        public byte Privilege { get; init; }

        public IB394APacketOut Into()
        {
            var packet = new Packet.Out.B394a.Login
            {
                LoginStatus = LoginStatus,
                Privilege = Privilege
            };

            return packet;
        }
    }
}