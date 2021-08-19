using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyJoin : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
            
            if (await lobbyProvider.IsPlayerInMatch(userContext.UserID))
                throw new UserAlreadyInMatchException();
            
            await userContext.SubscriptionManager.SubscribeToMatchUpdates(lobbyProvider); 
            var matches = await lobbyProvider.GetAvailableMatches();

            foreach (var match in matches)
                await connection.SendPacketAsync(new BanchoPacket(new MatchUpdate {MatchState = match}));

            await connection.SendPacketAsync(new BanchoPacket(new AutojoinChannelAvailable() {ChannelName= "#lobby"}));
        }
    }
}