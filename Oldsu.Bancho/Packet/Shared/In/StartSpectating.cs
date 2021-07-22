using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StartSpectating : ISharedPacketIn
    {
        public int UserID { get; set; }
        
        public async Task Handle(Client client)
        {
            await client.Server.AuthenticatedClients.ReadAsync(async clients =>
            {
                if (clients.TryGetValue((uint)UserID, out var host) &&
                    await client.StartSpectating(host))
                {
                    await host.SendPacketAsync(new BanchoPacket(new HostSpectatorJoined()
                    {
                        UserID = (int)await client.GetUserID(),
                    }));
                }
            });
        }
    }
}