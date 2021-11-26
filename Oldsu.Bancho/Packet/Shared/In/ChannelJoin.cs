using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.ChatChannel;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class ChannelJoin : ISharedPacketIn
    {
        public string ChannelName { get; set; }

        public void Handle(HubEventContext context)
        {
            if (ChannelName == "#lobby")
                return;

            if (ChannelName == "#multiplayer")
            { 
                if (context.User.Match == null)
                    throw new UserNotInMatchException();
                
                context.User.SendPacket(new ChannelJoined {ChannelName = "#multiplayer"});
                return;
            }

            if (context.Hub.AvailableChatChannels.TryGetValue(ChannelName, out var channel))
                channel.Join(context.User);
            else
                throw new InvalidChannelException();
        }
    }
}