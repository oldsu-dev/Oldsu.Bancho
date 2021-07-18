using System;
using System.IO;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using MatchType = System.IO.MatchType;

namespace Oldsu.Bancho.Packet.Objects.B904
{
    public class Match
    {
        public class SlotIDsSerializer
        {
            public void Serialize(object self, object instance, BinaryWriter writer)
            {
                int[] ids = (int[])self;
                Match match = (Match)instance;

                for (int i = 0; i < match.SlotStatus.Length; i++)
                    if ((match.SlotStatus[i] & Oldsu.Multiplayer.Enums.SlotStatus.HasPlayer) > 0)
                        writer.Write(ids[i]);
            }
            
            public object Deserialize(object instance, BinaryWriter writer)
            {
                throw new NotImplementedException();
            }
        }
        
        [BanchoSerializable] public byte MatchID { get; set; }
        [BanchoSerializable] public bool InProgress { get; set; }
        [BanchoSerializable] public MatchType MatchType { get; set; }
        [BanchoSerializable] public short ActiveMods { get; set; }
        [BanchoSerializable] public string GameName { get; set; }
        [BanchoSerializable] public string GamePassword { get; set; }
        [BanchoSerializable] public string BeatmapName { get; set; }
        [BanchoSerializable] public int BeatmapID { get; set; }
        [BanchoSerializable] public string BeatmapChecksum { get; set; }
        [BanchoSerializable(arrayElementCount: 8)] public SlotStatus[] SlotStatus { get; set; }
        [BanchoSerializable(arrayElementCount: 8)] public SlotTeams[] SlotTeams { get; set; }
        
        [BanchoCustomSerializer(typeof(SlotIDsSerializer))]
        [BanchoSerializable] 
        public int[] SlotIDs { get; set; }
        
        [BanchoSerializable] public int HostID { get; set; }
        [BanchoSerializable] public Mode PlayMode { get; set; }
        [BanchoSerializable] public MatchScoringTypes ScoringType { get; set; }
        [BanchoSerializable] public MatchTeamTypes TeamType { get; set; }
    }
}