using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Multiplayer.Enums;

namespace Oldsu.Bancho.Multiplayer.Objects
{
    public struct MatchSlot
    {
        public int SlotID { get; init; }
        public SlotStatus SlotStatus { get; set; }
        public SlotTeams SlotTeam { get; set; }
        public Client? Client { get; set; }

        public void Reset()
        {
            SlotStatus = SlotStatus.Open;
            SlotTeam = SlotTeams.Neutral;
            Client = null;
        }
    }
}