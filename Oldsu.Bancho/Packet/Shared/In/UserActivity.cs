using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public Activity Activity { get; set; }

        public async Task Handle(ConnectedUserContext userContext) =>
            await userContext.UserDataProvider.SetActivityAsync(userContext.UserID, Activity);
    }
}