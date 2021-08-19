using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLoad : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
            
            await userContext.SubscriptionManager.SubscribeToMatchGameUpdates(
                (await lobbyProvider.GetMatchGameObservable(userContext.UserID))!);
            
            await lobbyProvider.MatchLoad(userContext.UserID);
        }
    }
}   