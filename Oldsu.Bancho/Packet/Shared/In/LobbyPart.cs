using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyPart : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
            
            await userContext.SubscriptionManager.UnsubscribeFromMatchUpdates();

            // osu! leaves the lobby when joining a Match
            if (await lobbyProvider.GetMatchSetupObservable(userContext.UserID) is { } observable)
                await userContext.SubscriptionManager.SubscribeToMatchSetupUpdates(observable!);
            
            await connection.SendPacketAsync(new BanchoPacket(new ChannelLeft() {ChannelName = "#lobby"}));
        }
    }
}