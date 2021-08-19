using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchPart : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            await userContext.Dependencies.Get<ILobbyProvider>().TryLeaveMatch(userContext.UserID);

            await userContext.SubscriptionManager.UnsubscribeFromChannel("#multiplayer");
            await connection.SendPacketAsync(new BanchoPacket(new ChannelLeft() {ChannelName = "#multiplayer"}));
            
            if (!userContext.SubscriptionManager.SubscribedToLobby)
                await userContext.SubscriptionManager.UnsubscribeFromMatchUpdates();
        }
    }
}