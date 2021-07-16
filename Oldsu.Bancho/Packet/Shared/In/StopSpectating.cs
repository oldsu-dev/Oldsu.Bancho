using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StopSpectating : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            client.ClientContext!.SpectatorContext.StopSpecating();
        }
    }
}