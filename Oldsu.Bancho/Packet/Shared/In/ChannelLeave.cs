using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.ChatChannel;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class ChannelLeave : ISharedPacketIn
    {
        public string ChannelName { get; set; }

        public void Handle(HubEventContext context)
        {
            if (ChannelName is "#multiplayer" or "#lobby")
                return;
            
            if (context.Hub.AvailableChatChannels.TryGetValue(ChannelName, out var channel))
                channel.Leave(context.User!);
            else
                throw new InvalidChannelException();
        }
    }
}