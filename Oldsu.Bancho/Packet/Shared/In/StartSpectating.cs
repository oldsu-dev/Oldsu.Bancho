using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StartSpectating : ISharedPacketIn
    {
        public int UserID { get; set; }
        
        public async Task Handle(OnlineUser self)
        {
            OnlineUser host = null;
            
            if (!await self.ServerMediator.Users.ReadAsync(
                users => users.TryGetValue((uint) UserID, out host)))
            {
                return;
            }
            
            if (await self.StartSpectatingAsync(host!))
            {
                await host!.Connection.SendPacketAsync(new BanchoPacket(new HostSpectatorJoined()
                {
                    UserID = (int)self.UserInfo.UserID
                }));
            }
        }
    }
}