﻿using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using MatchType = System.IO.MatchType;

namespace Oldsu.Bancho.Multiplayer.Objects.B904
{
    public class Match
    {
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
        [BanchoSerializable(arrayElementCount: 8)] public int[] SlotIDs { get; set; }
        [BanchoSerializable] public int HostID { get; set; }
        [BanchoSerializable] public Mode PlayMode { get; set; }
        [BanchoSerializable] public MatchScoringTypes ScoringType { get; set; }
        [BanchoSerializable] public MatchTeamTypes TeamType { get; set; }
    }
}