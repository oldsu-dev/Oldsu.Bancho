using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.ChatChannel;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Exceptions.PacketHandling;
using Oldsu.Bancho.GameLogic;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendMessage : ISharedPacketIn
    {
        public string? Contents { get; init; }
        public string? Target { get; init; }
        
        public void Handle(HubEventContext context)
        {
            if (Contents == null || Target == null)
                throw new NullStringReceivedException();
            
            if (Target.StartsWith('#') && context.Hub.AvailableChatChannels.TryGetValue(Target, out var channel))
                channel.SendMessage(context.User, Contents);
            else if (Target == "#lobby")
                context.Hub.Lobby.SendMessage(context.User, Contents);
            else if (Target == "#multiplayer")
            {
                if (context.User.Match == null)
                    throw new UserNotInMatchException();
                
                context.User.Match.SendMessage(context.User, Contents);
            }
            else
                throw new InvalidChannelException();
        }
    }
}