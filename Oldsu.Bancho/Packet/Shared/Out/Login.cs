namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct Login : ISharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public int LoginStatus { get; init; }
        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.Login
            {
                LoginStatus = LoginStatus
            };

            return packet;
        }
    }
}