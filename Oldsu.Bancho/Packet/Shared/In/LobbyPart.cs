using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class LobbyPart : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            client.Server.MultiplayerLobby.RemovePlayer(client);
        }
    }
}