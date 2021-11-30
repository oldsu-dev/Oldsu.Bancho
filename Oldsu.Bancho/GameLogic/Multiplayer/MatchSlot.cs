using System;
using Oldsu.Bancho.GameLogic.Multiplayer.Enums;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet;

namespace Oldsu.Bancho.GameLogic.Multiplayer
{
    public class MatchSlot
    {
        public SlotStatus SlotStatus { get; set; }
        public SlotTeams SlotTeam { get; set; }
        public int UserID => (int?)User?.UserID ?? -1;
        public ScoreFrame? LastScoreFrame { get; set; }

        public User? User { get; private set; }

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
            User = null;
            LastScoreFrame = null;
        }
        
        public void ToggleLock()
        {
            SlotTeam = SlotTeams.Neutral;
            User = null;
            SlotStatus = SlotStatus == SlotStatus.Locked ? SlotStatus.Open : SlotStatus.Locked;
            LastScoreFrame = null;
        }
        
        public void SetUser(User user)
        {
            Reset();
            
            SlotStatus = SlotStatus.NotReady;
            SlotTeam = SlotTeams.Neutral;
            User = user;
        }

        public void Move(MatchSlot newSlot)
        {
            newSlot.SlotStatus = SlotStatus;
            newSlot.SlotTeam = SlotTeam;
            newSlot.User = User;
            newSlot.LastScoreFrame = LastScoreFrame;
            
            Reset();
        }
    }
}