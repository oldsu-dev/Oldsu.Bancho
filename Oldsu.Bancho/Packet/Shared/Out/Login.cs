namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Login : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public int LoginStatus { get; init; }
        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.Login
            {
                LoginStatus = LoginStatus
            };

            return packet;
        }
    }
}