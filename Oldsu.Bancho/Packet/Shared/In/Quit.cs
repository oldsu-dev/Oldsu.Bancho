using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Out.B904;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            Server.BroadcastPacket(new BanchoPacket(
                new UserQuit { UserID = (int)client.ClientInfo!.User.UserID })
            );
        }
    }
}