using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchComplete : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection connection) =>
            await userContext.Dependencies.Get<ILobbyProvider>().MatchComplete(userContext.UserID);
    }
}