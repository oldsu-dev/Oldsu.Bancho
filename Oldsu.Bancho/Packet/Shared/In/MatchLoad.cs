using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLoad : ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchLoad(userContext.UserID);
    }
}   