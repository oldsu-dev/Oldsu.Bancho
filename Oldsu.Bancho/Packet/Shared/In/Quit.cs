using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            client.Server.BroadcastPacket(new BanchoPacket(
                new UserQuit { UserID = (int)client.ClientContext!.User.UserID })
            );
        }
    }
}