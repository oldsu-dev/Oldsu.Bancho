using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public async Task Handle(UserContext context, Connection _)
        {
            switch (Target)
            {
                case "#multiplayer":
                    await context.Dependencies.Get<ILobbyProvider>()
                        .SendMessageToMatch(context.UserID, context.Username, Contents);
                    break;
                case "#lobby":                    
                    await context.Dependencies.Get<ILobbyProvider>()
                        .SendMessageToLobby(context.Username, Contents);
                    break;
                default:
                    var channel = await context.Dependencies.Get<IChatProvider>().GetChannel(Target, context.Privileges);
                    if (channel is not null)
                        await channel.SendMessage(context.Username, Contents);
                    break;
            }
        }
    }
}