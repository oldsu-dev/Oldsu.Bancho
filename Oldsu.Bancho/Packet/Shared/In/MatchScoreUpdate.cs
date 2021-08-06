using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchScoreUpdate : ISharedPacketIn
    {
        public ScoreFrame ScoreFrame { get; set; }

        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchScoreUpdate(userContext.UserID, ScoreFrame);
    }
}