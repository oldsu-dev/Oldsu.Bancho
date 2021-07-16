using System;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Bancho.Multiplayer.Objects;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;

namespace Oldsu.Bancho.Multiplayer
{
    public class Match
    {
        public const int MaxMatchSize = 8;
        
        public byte MatchID { get; set; }
        public int HostID { get; set; }
        
        public string GameName { get; set; }
        public string GamePassword { get; set; }
        public string AllowedVersions { get; set; } // <- WTF

        public string BeatmapName { get; set; }
        public int BeatmapID { get; set; }
        public string BeatmapChecksum { get; set; }
        public Mode PlayMode { get; set; }
        public MatchScoringTypes ScoringType { get; set; }
        public MatchTeamTypes TeamType { get; set; }
        
        public bool InProgress { get; set; }
        public MatchType MatchType { get; set; }
        public short ActiveMods { get; set; }

        public MatchSlot[] MatchSlots { get; set; } = new MatchSlot[8];

        public Match()
        {
            for (int x = 0; x < MaxMatchSize; x++)
            {
                MatchSlots[x] = new MatchSlot
                {
                    SlotID = x,
                    SlotStatus = SlotStatus.Open,
                    SlotTeam = SlotTeams.Neutral,
                    Client = null
                };
            }
        }

        public int? Join(int clientId, string? password)
        {
            throw new NotImplementedException();
        }

        public bool Leave(int clientId)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void MoveSlot()
        {
            
        }
    }
}