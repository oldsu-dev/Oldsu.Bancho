using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public Activity Activity { get; set; }

        public async Task Handle(UserContext userContext, Connection _) =>
            await userContext.Dependencies.Get<IUserStateProvider>().SetActivityAsync(userContext.UserID, Activity);
    }
}