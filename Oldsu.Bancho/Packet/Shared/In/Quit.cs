using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            await client.Server.BroadcastPacketAsync(new BanchoPacket(
                new UserQuit { UserID = (int)await client.GetUserID() })
            );
        }
    }
}