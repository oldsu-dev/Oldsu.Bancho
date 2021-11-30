using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Multiplayer;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }

        public void Handle(HubEventContext context)
        {
            if (context.User.Match != null)
                throw new UserAlreadyInMatchException();
            
            context.Hub.Lobby.TryCreateMatch(context.User, MatchSettings);
        }
    }
}