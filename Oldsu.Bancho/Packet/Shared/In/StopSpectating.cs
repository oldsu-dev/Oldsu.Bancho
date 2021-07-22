using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StopSpectating : ISharedPacketIn
    {
        public async Task Handle(Client client) => await client.StopSpectating();
    }
}