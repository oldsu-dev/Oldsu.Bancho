using System.IO;
using System.Linq;
using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Bancho.GameLogic.Multiplayer.Enums;
using Oldsu.Enums;
using MatchType = Oldsu.Bancho.GameLogic.Multiplayer.Enums.MatchType;

namespace Oldsu.Bancho.Packet.Objects.B904
{
    public class SlotIDsSerializer : IBanchoCustomSerializer
    {
        public void Serialize(object self, object instance, BinaryWriter writer)
        {
            int[] ids = (int[])self;
            MatchState matchState = (MatchState)instance;

            for (int i = 0; i < matchState.SlotStatus.Length; i++)
                if ((matchState.SlotStatus[i] & SlotStatus.HasPlayer) > 0)
                    writer.Write(ids[i]);
        }

        public object Deserialize(object instance, BinaryReader reader)
        {
            MatchState matchState = (MatchState)instance;
            int[] ids = new int[8];

            for (int i = 0; i < matchState.SlotStatus.Length; i++)
                ids[i] = (matchState.SlotStatus[i] & SlotStatus.HasPlayer) > 0
                    ? reader.ReadInt32()
                    : -1;

            return ids;
        }
    }    
    
    public struct MatchState
    {
        public MatchSettings ToMatchSettings()
        {
            return new MatchSettings
            {
                ActiveMods = ActiveMods,
                BeatmapChecksum = BeatmapChecksum,
                BeatmapName = BeatmapName,
                GameName = GameName,
                GamePassword = GamePassword,
                MatchType = MatchType,
                PlayMode = PlayMode,
                ScoringType = ScoringType,
                TeamType = TeamType,
                BeatmapID = BeatmapID
            };
        }
        
        [BanchoSerializable] public byte MatchID;
        [BanchoSerializable] public bool InProgress;
        [BanchoSerializable] public MatchType MatchType;
        [BanchoSerializable] public short ActiveMods;
        [BanchoSerializable] public string GameName;
        [BanchoSerializable] public string GamePassword;
        [BanchoSerializable] public string BeatmapName;
        [BanchoSerializable] public int BeatmapID;
        [BanchoSerializable] public string BeatmapChecksum;
        [BanchoSerializable(arrayElementCount: 8)] public SlotStatus[] SlotStatus;
        [BanchoSerializable(arrayElementCount: 8)] public SlotTeams[] SlotTeams;
        
        [BanchoCustomSerializer(typeof(SlotIDsSerializer))]
        [BanchoSerializable] 
        public int[] SlotIDs;

        [BanchoSerializable] public int HostID;
        [BanchoSerializable] public Mode PlayMode;
        [BanchoSerializable] public MatchScoringTypes ScoringType;
        [BanchoSerializable] public MatchTeamTypes TeamType;
        
        public static MatchState FromMatchState(Match matchData)
        {
            return new MatchState
            {
                ActiveMods = matchData.Settings.ActiveMods,
                BeatmapChecksum = matchData.Settings.BeatmapChecksum!,
                BeatmapName = matchData.Settings.BeatmapName!,
                BeatmapID = matchData.Settings.BeatmapID,
                GameName = matchData.Settings.GameName,
                GamePassword = matchData.Settings.GamePassword!,
                InProgress = matchData.InProgress,
                MatchType = matchData.Settings.MatchType,
                PlayMode = matchData.Settings.PlayMode,
                ScoringType = matchData.Settings.ScoringType,
                SlotStatus = matchData.MatchSlots.Select(status => status.SlotStatus).ToArray(),
                SlotTeams = matchData.MatchSlots.Select(status => status.SlotTeam).ToArray(),
                SlotIDs = matchData.MatchSlots.Select(status => status.UserID).ToArray(),
                TeamType = matchData.Settings.TeamType,
                HostID = matchData.HostID,
                MatchID = (byte)matchData.MatchID
            };
        }
    }
}