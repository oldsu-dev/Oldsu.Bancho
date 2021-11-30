using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchJoin : ISharedPacketIn
    {
        public uint MatchID { get; set; }
        public string GamePassword { get; set; }

        public void Handle(HubEventContext context)
        {
            if (context.User.Match != null)
                throw new UserAlreadyInMatchException();

            context.Hub.Lobby.GetMatchByID(MatchID).TryJoin(context.User, GamePassword);
        }
    }
}