using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(ConnectedUserContext userContext)
        {
            await using var database = new Database();
            var stats = await database.GetStatsWithRankAsync(userContext.UserID, 0);
            
            await userContext.UserDataProvider.SetStatsAsync(userContext.UserID, stats);
        }
    }
}