using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection _)
        {
            await using var database = new Database();
            var stats = await database.GetStatsWithRankAsync(userContext.UserID, 0);
            
            await userContext.Dependencies.Get<IUserStateProvider>().SetStatsAsync(userContext.UserID, stats);
        }
    }
}