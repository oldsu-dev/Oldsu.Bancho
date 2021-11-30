using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.GameLogic.Events
{
    public class HubEventConnect : HubEvent
    {
        public HubEventConnect(User invoker) : base(invoker)
        {
        }

        public override void Handle(HubEventContext context)
        {
            if (context.Hub.UserPanelManager.IsOnline(context.User.UserID))
            {
                var oldUser = context.Hub.UserPanelManager.EntitiesByUserID[context.User.UserID].User;
                
                new HubEventDisconnect(oldUser)
                    .Handle(new HubEventContext(context.Hub, context.HubEventLoop, oldUser));
            }

            context.Hub.UserPanelManager.RegisterUser(context.User);
            
            foreach (var channel in context.Hub.AvailableChatChannels.Values)
            {
                if (channel.AutoJoin)
                {
                    context.User.SendPacket(new AutojoinChannelAvailable{ChannelName = channel.Tag});
                    channel.Join(context.User);
                }
                else
                {
                    context.User.SendPacket(new ChannelAvailable{ChannelName = channel.Tag});
                }
            }
            
            context.HubEventLoop.SendEvent(new HubEventPacket(context.User, new UserStatsRequest()));
        }
    }
}