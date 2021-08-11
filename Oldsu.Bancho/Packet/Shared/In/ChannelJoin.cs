using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
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
                    await userContext.SubscriptionManager.SubscribeToChannel(
                        await userContext.LobbyProvider.GetMatchChatChannel(userContext.UserID));
                
                    await connection.SendPacketAsync(new BanchoPacket(new ChannelJoined()
                        {ChannelName = "#multiplayer"}));
                    break;
                
                case "#lobby":
                    await userContext.SubscriptionManager.SubscribeToChannel(
                        await userContext.LobbyProvider.GetLobbyChatChannel());
                    
                    await connection.SendPacketAsync(new BanchoPacket(new ChannelJoined()
                        {ChannelName = "#lobby"}));
                    break;
                default:
                    var channel = await userContext.ChatProvider.GetChannel(ChannelName, userContext.Privileges);
                    if (channel is not null)
                        await userContext.JoinChannel(channel);
                    break;
            }
        }
    }
}