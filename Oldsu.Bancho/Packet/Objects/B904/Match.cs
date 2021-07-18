using System;
using System.IO;
using System.Security.Permissions;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using MatchType = Oldsu.Bancho.Multiplayer.Enums.MatchType;

namespace Oldsu.Bancho.Packet.Objects.B904
{
    public class SlotIDsSerializer : IBanchoCustomSerializer
    {
        public void Serialize(object self, object instance, BinaryWriter writer)
        {
            int[] ids = (int[])self;
            Match match = (Match)instance;

            for (int i = 0; i < match.SlotStatus.Length; i++)
                if ((match.SlotStatus[i] & Oldsu.Multiplayer.Enums.SlotStatus.HasPlayer) > 0)
                    writer.Write(ids[i]);
        }

        public object Deserialize(object instance, BinaryReader reader)
        {
            Match match = (Match)instance;
            int[] ids = new int[8];

            for (int i = 0; i < match.SlotStatus.Length; i++)
                ids[i] = (match.SlotStatus[i] & Oldsu.Multiplayer.Enums.SlotStatus.HasPlayer) > 0
                    ? reader.ReadInt32()
                    : -1;

            return ids;
        }
    }    
    
    public struct Match
    {
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
    }
}