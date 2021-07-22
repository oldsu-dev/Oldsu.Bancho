using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public async Task Handle(Client client)
        {
            await client.ClientContext!.ReadAsync(async context =>
            {
                await client.Server.BroadcastPacketToOthersAsync(new BanchoPacket(new Out.SendMessage
                {
                    Sender = context.User.Username,
                    Contents = Contents,
                    Target = Target
                }), context.User.UserID);
            });
        }
    }
}