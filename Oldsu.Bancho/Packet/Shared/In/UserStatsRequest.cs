using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(OnlineUser self)
        {
            using var statsLock = await self.Stats.AcquireReadLockGuard();
            using var activityLock = await self.Activity.AcquireReadLockGuard();
            
            await self.Connection.SendPacketAsync(new BanchoPacket(
                new StatusUpdate
                {
                    Stats = ~statsLock, Activity = ~activityLock, 
                    Presence = self.Presence, User = self.UserInfo,
                    Completeness = Completeness.Self
                })
            );
        }
    }
}