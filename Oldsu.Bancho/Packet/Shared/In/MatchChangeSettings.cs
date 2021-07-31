using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSettings : ISharedPacketIn
    {
        public string GameName { get; init; }
        public string GamePassword { get; init; }
        public string BeatmapName { get; init; }
        public string BeatmapChecksum { get; init; }
        public int BeatmapID { get; init; }
        public MatchType MatchType { get; init; }
        public short ActiveMods { get; init; }
        
        public async Task Handle(OnlineUser self)
        {
            if (self.MatchMediator is {} matchMediator)
            {
                await matchMediator.CurrentMatch.WriteAsync(match =>
                {
                    if (match.HostID != self.UserInfo.UserID)
                        return;

                    match.ActiveMods = ActiveMods;
                    match.BeatmapID = BeatmapID;
                    match.MatchType = MatchType;
                    match.BeatmapChecksum = BeatmapChecksum;
                    match.BeatmapName = BeatmapName;
                    match.GamePassword = GamePassword;
                    match.GameName = GameName;
                });
            }
        }
    }
}