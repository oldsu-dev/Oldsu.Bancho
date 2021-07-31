using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public Activity Activity { get; set; }

        public async Task Handle(OnlineUser self)
        {
            await self.Activity.SetValueAsync(Activity); 

            await self.ServerMediator.Users.ReadAsync(async clients =>
            {
                using var statsLock = await self.Stats.AcquireReadLockGuard();
                using var activityLock = await self.Activity.AcquireReadLockGuard();

                clients.BroadcastPacket(new BanchoPacket( 
                    new StatusUpdate
                    {
                        Stats = ~statsLock, Activity = ~activityLock, 
                        Presence = self.Presence, User = self.UserInfo,
                        Completeness = Completeness.Self
                    })
                );
            });
        }
    }
}