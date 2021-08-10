﻿using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Multiplayer.Enums;

namespace Oldsu.Bancho.Multiplayer
{
    public class MatchSlot
    {
        public SlotStatus SlotStatus { get; set; }
        public SlotTeams SlotTeam { get; set; }
        public int UserID { get; set; }
        
        public bool Loaded { get; set; }
        public bool Skipped { get; set; }
        public bool Completed { get; set; }

        public MatchSlot()
        {
            Reset();
        }

        public void Reset()
        {
            SlotStatus = SlotStatus.Open;
            SlotTeam = SlotTeams.Neutral;
            UserID = -1;
        }
        
        public void ToggleLock()
        {
            SlotTeam = SlotTeams.Neutral;
            UserID = -1;
            SlotStatus = SlotStatus == SlotStatus.Locked ? SlotStatus.Open : SlotStatus.Locked;
        }
        
        public void SetUser(int userId)
        {
            SlotStatus = SlotStatus.NotReady;
            SlotTeam = SlotTeams.Neutral;
            UserID = userId;
        }

        public void Move(MatchSlot newSlot)
        {
            newSlot.SlotStatus = SlotStatus;
            newSlot.SlotTeam = SlotTeam;
            newSlot.UserID = UserID;
            
            Reset();
        }
    }
}