using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Objects;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchScoreUpdate : ISharedPacketIn
    {
        public ScoreFrame ScoreFrame { get; set; }

        public void Handle(HubEventContext context)
        {
            if (context.User!.Match == null)
                throw new UserNotInMatchException();
            
            context.User.Match.ScoreUpdate(context.User, ScoreFrame);
        }
    }
}