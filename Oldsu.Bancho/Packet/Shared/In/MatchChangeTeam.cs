using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeTeam : ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.Dependencies.Get<ILobbyProvider>().MatchChangeTeam(userContext.UserID);
    }
}