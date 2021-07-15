namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SendMessage : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public string Sender { get; init; }
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.SendMessage
            {
                Sender = Sender,
                Contents = Contents,
                Target = Target
            };

            return packet;
        }
    }
}