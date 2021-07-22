using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Bancho.Packet;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using Oldsu.Utils;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Multiplayer
{
    public class Match
    {
        public const int MaxMatchSize = 8;
        
        public byte MatchID { get; set; }
        public int HostID { get; set; }
        
        public string GameName { get; set; }
        public string? GamePassword { get; set; }
        public HashSet<Version> AllowedVersions { get; } = new HashSet<Version> { Version.B394A, Version.B904 }; // <- WTF

        public string? BeatmapName { get; set; }
        public int BeatmapID { get; set; }
        public string? BeatmapChecksum { get; set; }
        
        public Mode PlayMode { get; set; }
        public MatchScoringTypes ScoringType { get; set; }
        public MatchTeamTypes TeamType { get; set; }
        
        public bool InProgress { get; set; }
        public MatchType MatchType { get; set; }
        public short ActiveMods { get; set; }

        public MatchSlot[] MatchSlots { get; set; }

        public void UpdateSupportedVersions()
        {
            // b394 lacks password field in bMatch, so it wont be in the lobby.
            if (GamePassword is not (null or ""))
                AllowedVersions.Remove(Version.B394A);
        }

        public Match(string gameName, string gamePassword, int beatmapId, 
            string? beatmapName, string? beatmapChecksum)
        {
            GameName = gameName;
            GamePassword = gamePassword;
            BeatmapName = beatmapName;
            BeatmapChecksum = beatmapChecksum;
            BeatmapID = beatmapId;

            MatchSlots = new MatchSlot[MaxMatchSize];

            Array.Fill(MatchSlots, 
                new MatchSlot { SlotId = -1, Client = null, SlotStatus = SlotStatus.Open, SlotTeam = SlotTeams.Neutral});
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="password"></param>
        /// <returns>Slot ID</returns>
        public async Task<int?> TryJoin(Client client, string? password)
        {
            if (password != GamePassword)
                return -1;
            
            for (int i = 0; i < MaxMatchSize; i++)
            {
                if (MatchSlots[i].Client != null) 
                    continue;
                
                MatchSlots[i].SlotStatus = SlotStatus.NotReady;
                MatchSlots[i].SlotTeam = TeamType is MatchTeamTypes.TeamVs or MatchTeamTypes.TagTeamVs ? 
                    SlotTeams.Blue : SlotTeams.Red;
                
                MatchSlots[i].Client = client;
                MatchSlots[i].SlotId = (int)await client.GetUserID();

                return i;
            }
            
            return null;
        }

        public void Leave(int slotId)
        {
            MatchSlots[slotId].Reset();
        }

        public void Start()
        {
            Array.ForEach(MatchSlots, slot =>
            {
                slot.SlotStatus = SlotStatus.Playing;
                slot.Completed = false;
                slot.Loaded = false;
                slot.Skipped = false;
            });
        }

        public void ForceStop()
        {            
            for (int i = 0; i < MaxMatchSize; i++)
                Complete(i);
        }

        public void UnreadyAll()
        {
            Array.ForEach(MatchSlots, slot => slot.SlotStatus = SlotStatus.NotReady);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Wether is skipped by all the users or not.</returns>
        public bool Skip(int slotId)
        {
            MatchSlots[slotId].Skipped = true;
            return MatchSlots.All(slot => slot.Skipped);
        }

        public bool CompleteLoad(int slotId)
        {
            MatchSlots[slotId].Loaded = true;
            return MatchSlots.All(slot => slot.Loaded);
        }
        
        public bool Complete(int slotId)
        {
            MatchSlots[slotId].Completed = true;
            return MatchSlots.All(slot => slot.Completed);
        }

        public bool MoveSlot(int currentSlot, int newSlot)
        {
            if (MatchSlots[newSlot].Client != null)
                return false;

            MatchSlots[currentSlot].Move(ref MatchSlots[newSlot]);
            return true;
        }

        public bool TransferHost(int currentSlot, int newSlot)
        {
            if (MatchSlots[newSlot].Client == null ||
                MatchSlots[newSlot].SlotId == MatchSlots[currentSlot].SlotId)
            {
                return false;
            }

            HostID = MatchSlots[newSlot].SlotId;
            return true;
        }
    }
}