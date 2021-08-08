using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyPart : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection _)
        {
            await userContext.SubscriptionManager.UnsubscribeFromMatchUpdates();

            // osu! leaves the lobby when joining a Match
            if (await userContext.LobbyProvider.GetMatchSetupObservable(userContext.UserID) is { } observable)
                await userContext.SubscriptionManager.SubscribeToMatchSetupUpdates(observable!);
        }
    }
}