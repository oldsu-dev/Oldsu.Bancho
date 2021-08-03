using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Multiplayer.Enums;

namespace Oldsu.Bancho.Multiplayer
{
    public struct MatchSlot
    {
        public SlotStatus SlotStatus { get; set; }
        public SlotTeams SlotTeam { get; set; }
        public int UserID { get; set; }
        
        public bool Loaded { get; set; }
        public bool Skipped { get; set; }
        
        public bool Completed
        {
            get => (SlotStatus & SlotStatus.Complete) > 0;
            set => SlotStatus &= value ? SlotStatus.Complete : 0;
        }

        public void Reset()
        {
            SlotStatus = SlotStatus.Open;
            SlotTeam = SlotTeams.Neutral;
        }
        
        public void Move(ref MatchSlot newSlot)
        {
            newSlot.SlotStatus = SlotStatus;
            newSlot.SlotTeam = SlotTeam;
            
            Reset();
        }
    }
}