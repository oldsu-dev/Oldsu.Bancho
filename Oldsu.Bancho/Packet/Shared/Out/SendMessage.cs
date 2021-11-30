namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class SendMessage : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string Sender { get; init; }
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public IGenericPacketOut IntoPacket()
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