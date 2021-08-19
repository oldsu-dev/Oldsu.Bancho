using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public struct MatchSkip : ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.Dependencies.Get<ILobbyProvider>().MatchSkip(userContext.UserID);
    }
}