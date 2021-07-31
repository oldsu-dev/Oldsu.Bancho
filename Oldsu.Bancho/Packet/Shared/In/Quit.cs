using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public async Task Handle(OnlineUser self)
        {
            await self.ServerMediator.Users.ReadAsync(users =>
                users.BroadcastPacket(new BanchoPacket(
                    new UserQuit {UserID = (int) self.UserInfo.UserID})
                ));
        }
    }
}