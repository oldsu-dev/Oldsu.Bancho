using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class ChannelLeave : ISharedPacketIn
    {
        public string ChannelName { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            switch (ChannelName)
            {
                case "#multiplayer":
                    await userContext.SubscriptionManager.UnsubscribeFromChannel("#multiplayer");
                    await connection.SendPacketAsync(new BanchoPacket(new ChannelLeft()
                    {
                        ChannelName = "#multiplayer"
                    }));
                    break;
                
                case "lobby":
                    await userContext.SubscriptionManager.UnsubscribeFromChannel("#lobby");
                    await connection.SendPacketAsync(new BanchoPacket(new ChannelLeft()
                    {
                        ChannelName = "#lobby"
                    }));
                    break;
                default:
                {
                    var chatProvider = userContext.Dependencies.Get<IChatProvider>();
                    
                    var channel = await chatProvider.GetChannel(ChannelName, userContext.Privileges);
                    if (channel is not null)
                        await userContext.LeaveChannel(channel);
                } break;
            }
        }
    }
}