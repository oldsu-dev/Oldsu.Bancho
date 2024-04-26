using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchUnready : ISharedPacketIn
    {
        public void Handle(HubEventContext context)
        {
            if (context.User!.Match == null)
                throw new UserNotInMatchException();
            
            context.User.Match.Unready(context.User);
        }
    }
}