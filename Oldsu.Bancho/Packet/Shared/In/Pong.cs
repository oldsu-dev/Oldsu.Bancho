using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Pong : ISharedPacketIn
    {
        public async Task Handle(UserContext client, Connection connection)
        {
            await connection.SendPacketAsync(new BanchoPacket(new Ping()));
        }
    }
}