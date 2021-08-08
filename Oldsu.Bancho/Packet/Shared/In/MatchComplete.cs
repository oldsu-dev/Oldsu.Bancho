using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchComplete : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection)
        {
            await userContext.LobbyProvider.MatchComplete(userContext.UserID);
        }
    }
}