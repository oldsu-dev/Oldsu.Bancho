using System.Linq;
using Oldsu.Bancho.Multiplayer;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class MatchJoinSuccess : ISharedPacketOut, Into<IB904PacketOut>
    {
        public Match Match { get; set; }
        
        public IB904PacketOut Into()
        {
            using var slots = Match.Slots;
            
            return new Packet.Out.B904.MatchJoinSuccess
            {
                Match = new Objects.B904.Match
                {
                    ActiveMods = Match.ActiveMods,
                    BeatmapChecksum = Match.BeatmapChecksum!,
                    BeatmapID = Match.BeatmapID,
                    BeatmapName = Match.BeatmapName!,
                    GameName = Match.GameName!,
                    GamePassword = Match.GamePassword!,
                    MatchType = Match.MatchType,
                    InProgress = Match.InProgress,
                    PlayMode = Match.PlayMode,
                    ScoringType = Match.ScoringType,
                    SlotStatus = slots.Select(slot => slot.SlotStatus).ToArray(),
                    SlotTeams = slots.Select(slot => slot.SlotTeam).ToArray(),
                    SlotIDs = slots.Select(slot => ((int?)slot.Client?.UserID) ?? -1).ToArray(),
                    TeamType = Match.TeamType,
                    HostID = Match.HostID,
                    MatchID = Match.MatchID
                }
            };
        }
    }
}