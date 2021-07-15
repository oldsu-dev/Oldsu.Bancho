using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public async Task Handle(Client client)
        {
            Server.BroadcastPacket(new BanchoPacket(new Out.SendMessage
            {
                Sender = client.ClientInfo!.User.Username,
                Contents = Contents,
                Target = Target
            }));
        }
    }
}