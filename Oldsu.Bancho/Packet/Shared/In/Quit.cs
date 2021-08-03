using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public Task Handle(UserContext context, Connection connection)
        {
            connection.Disconnect();
            
            return Task.CompletedTask;
        }
    }
}