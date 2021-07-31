using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyJoin : ISharedPacketIn
    {
        public async Task Handle(OnlineUser self) =>
            await self.ServerMediator.Lobby.WriteAsync(lobby => lobby.AddPlayer(self));
        
    }
}