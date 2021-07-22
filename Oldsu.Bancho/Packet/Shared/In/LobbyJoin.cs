using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyJoin : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            await client.Server.MultiplayerLobby.WriteAsync(lobby =>
                lobby.AddPlayerAsync(client));
        }
    }
}