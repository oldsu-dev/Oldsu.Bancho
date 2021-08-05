using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchUnready : ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchSetUnready(userContext.UserID);
    }
}