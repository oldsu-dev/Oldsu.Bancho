using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class ChannelJoin : ISharedPacketIn
    {
        public string ChannelName { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            switch (ChannelName)
            {
                case "#multiplayer":
                {
                    var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
                    
                    await userContext.SubscriptionManager.SubscribeToChannel(
                        await lobbyProvider.GetMatchChatChannel(userContext.UserID));

                    await connection.SendPacketAsync(new BanchoPacket(new ChannelJoined()
                        {ChannelName = "#multiplayer"}));
                } break;

                case "#lobby":
                {
                    var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
                    
                    await userContext.SubscriptionManager.SubscribeToChannel(
                        await lobbyProvider.GetLobbyChatChannel());

                    await connection.SendPacketAsync(new BanchoPacket(new ChannelJoined()
                        {ChannelName = "#lobby"}));
                } break;

                default:
                {
                    var chatProvider = userContext.Dependencies.Get<IChatProvider>();
                    
                    var channel = await chatProvider.GetChannel(ChannelName, userContext.Privileges);
                    if (channel is not null)
                        await userContext.JoinChannel(channel);
                } break;
            }
        }
    }
}