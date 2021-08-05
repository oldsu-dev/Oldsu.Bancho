using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeMods : ISharedPacketIn
    {
        public int Mods { get; set; }

        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchChangeMods(userContext.UserID, (short) Mods);
    }
}