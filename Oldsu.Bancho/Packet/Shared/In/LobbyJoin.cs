using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyJoin : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            await userContext.SubscriptionManager.SubscribeToMatchUpdates(userContext.LobbyProvider); 
            var matches = await userContext.LobbyProvider.GetAvailableMatches();

            foreach (var match in matches)
                await connection.SendPacketAsync(new BanchoPacket(new MatchUpdate {MatchState = match}));
        }
    }
}