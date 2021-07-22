using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public string GameName { get; set; }
        public string GamePassword { get; set; }
        public string BeatmapName { get; set; }
        public string BeatmapChecksum { get; set; }
        public int BeatmapID { get; set; }
        
        public async Task Handle(Client client)
        {
            Match match = new Match(GameName, GamePassword, BeatmapID, BeatmapName, BeatmapChecksum);
            _ = match.TryJoin(client, GamePassword);

            match.HostID = (int)await client.GetUserID();

            await client.Server.MultiplayerLobby.WriteAsync(async lobby =>
            {
                if (lobby.RegisterMatch(match, out var matchWrapper))
                {
                    await client.ClientContext!.WriteAsync(context => context.MultiplayerContext.Match = matchWrapper);
                    await client.SendPacketAsync(new BanchoPacket(new MatchJoinSuccess { Match = match }));
                }
            });
        }
    }
}