using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSettings : ISharedPacketIn
    {
        public string GameName { get; set; }
        public string GamePassword { get; set; }
        public string BeatmapName { get; set; }
        public string BeatmapChecksum { get; set; }
        public int BeatmapID { get; set; }
        public MatchType MatchType { get; set; }
        public short ActiveMods { get; set; }
        
        public async Task Handle(Client client)
        {
            await client.ClientContext!.ReadAsync(async context =>
            {
                await context.MultiplayerContext.Match!.WriteAsync(match =>
                {
                    if (match.HostID != context.User.UserID)
                        return;

                    match.ActiveMods = ActiveMods;
                    match.BeatmapID = BeatmapID;
                    match.MatchType = MatchType;
                    match.BeatmapChecksum = BeatmapChecksum;
                    match.BeatmapName = BeatmapName;
                    match.GamePassword = GamePassword;
                    match.GameName = GameName;
                });
            });
        }
    }
}