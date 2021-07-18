using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public Match Match { get; set; }
        
        public async Task Handle(Client client)
        {
            if (client.Server.MultiplayerLobby.RegisterMatch(Match))
            {
                _ = Match.TryJoin(client, Match.GamePassword);
                client.ClientContext!.MultiplayerContext.Match = Match;
            
                await client.SendPacketAsync(new BanchoPacket(new MatchJoinSuccess{ Match = Match }));
            }
        }
    }
}