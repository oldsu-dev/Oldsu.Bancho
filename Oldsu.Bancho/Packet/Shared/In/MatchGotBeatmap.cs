using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchGotBeatmap : ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.Dependencies.Get<ILobbyProvider>().MatchGotBeatmap(userContext.UserID);
    }
}