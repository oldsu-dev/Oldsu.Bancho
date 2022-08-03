using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Multiplayer;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSettings : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }

        public void Handle(HubEventContext context)
        {
            if (context.User!.Match == null)
                throw new UserNotInMatchException();
            
            context.User.Match.ChangeSettings(context.User, MatchSettings);
        }
    }
}