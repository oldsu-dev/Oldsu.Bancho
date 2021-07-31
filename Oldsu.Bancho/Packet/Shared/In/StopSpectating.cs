using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StopSpectating : ISharedPacketIn
    {
        public async Task Handle(OnlineUser self)
        {
            var targetUser = await self.StopSpectatingAsync();

            if (targetUser != null)
            {
                await self.Connection.SendPacketAsync(
                    new BanchoPacket(new HostSpectatorLeft {UserID = (int)targetUser.UserInfo.UserID}));
            }
        }
    }
}