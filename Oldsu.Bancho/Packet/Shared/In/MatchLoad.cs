using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLoad : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            await userContext.SubscriptionManager.SubscribeToMatchGameUpdates(
                (await userContext.LobbyProvider.GetMatchGameObservable(userContext.UserID))!);
            
            await userContext.LobbyProvider.MatchLoad(userContext.UserID);
        }
    }
}   