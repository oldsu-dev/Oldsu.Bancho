using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendPrivateMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }

        public async Task Handle(UserContext context, Connection _)
        {
            var channel = await context.ChatProvider.GetUserChannel(Target);

            if (channel is not null)
                await channel.SendMessage(context.Username, Contents);
        }
    }
}