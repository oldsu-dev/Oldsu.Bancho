using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StartSpectating : ISharedPacketIn
    {
        public int UserID;
        
        public async Task Handle(Client client)
        {
            if (client.Server.AuthenticatedClients.TryGetValue((uint)UserID, out var host))
                client.ClientContext!.SpectatorContext.StartSpectating(host);
        }
    }
}