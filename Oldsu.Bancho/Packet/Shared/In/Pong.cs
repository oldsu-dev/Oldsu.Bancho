using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Pong : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            await client.SendPacket(new BanchoPacket(new Ping()));
        }
    }
}