using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyJoin : ISharedPacketIn
    {
        public void Handle(HubEventContext context) =>
            context.Hub.Lobby.Join(context.User);
    }
}