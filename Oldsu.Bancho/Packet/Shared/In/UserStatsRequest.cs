using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection _)
        {
            var userProvider = userContext.Dependencies.Get<IUserStateProvider>();
            var userData = await userProvider.GetUser(userContext.UserID);
            
            var gamemode = userData!.Activity is ActivityWithBeatmap activityWithBeatmap
                ? activityWithBeatmap.GameMode : (byte)Mode.Standard;
            
            await using var database = new Database();
            var stats = await database.GetStatsWithRankAsync(userContext.UserID, gamemode);
            
            await userContext.Dependencies.Get<IUserStateProvider>().SetStatsAsync(userContext.UserID, (Mode)gamemode, stats);
        }
    }
}