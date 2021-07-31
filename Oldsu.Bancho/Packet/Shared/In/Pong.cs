using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Pong : ISharedPacketIn
    {
        public async Task Handle(OnlineUser client)
        {
            await client.Connection.SendPacketAsync(new BanchoPacket(new Ping()));
        }
    }
}