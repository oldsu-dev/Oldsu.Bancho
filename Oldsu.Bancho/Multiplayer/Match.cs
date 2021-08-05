using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Bancho.Packet;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using Oldsu.Utils;
using Oldsu.Utils.Threading;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Multiplayer
{
    public class MatchSettings : ICloneable
    {
        public string? BeatmapName { get; set; }
        public int BeatmapID { get; set; }
        public string? BeatmapChecksum { get; set; }
        
        public Mode PlayMode { get; set; }
        public MatchScoringTypes ScoringType { get; set; }
        public MatchTeamTypes TeamType { get; set; }
        
        public string GameName { get; set; }
        public string? GamePassword { get; set; }
        
        public MatchType MatchType { get; set; }
        public short ActiveMods { get; set; }
        public object Clone() => MemberwiseClone();
    }
    
    public class MatchState : ICloneable
    {
        public const int MaxMatchSize = 8;
        
        public int MatchID { get; set; }
        public int HostID { get; set; }
        
        public HashSet<Version> AllowedVersions { get; }  // <- WTF

        public bool InProgress { get; set; }

        public MatchSettings Settings { get; private set; }
        public MatchSlot[] MatchSlots { get; }

        public bool IsEmpty => MatchSlots.All(slot => slot.UserID == -1);
        
        private void UpdateSupportedVersions()
        {
            // Used for compatibility with future versions
        }
        
        public bool ChangeSettings(int slotId, MatchSettings settings)
        {
            if (MatchSlots[slotId].UserID != HostID)
                return false;

            var password = settings.GamePassword;
            
            Settings = settings;
            UpdateSupportedVersions();

            settings.GamePassword = password;
            
            return true;
        }
        
        public MatchState(int matchId, int hostId, MatchSettings settings)
        {
            AllowedVersions = new HashSet<Version> { Version.B904 };

            Settings = settings;
            UpdateSupportedVersions();
            
            MatchSlots = new MatchSlot[MaxMatchSize];

            MatchID = matchId;
            HostID = hostId;

            Array.Fill(MatchSlots, new MatchSlot
            {
                UserID = -1, 
                SlotStatus = SlotStatus.Open, 
                SlotTeam = SlotTeams.Neutral
            });
        }

        public uint? Join(int userId, string password)
        {
            if (password != Settings.GamePassword)
                return null;
            
            var newSlotIndex = Array.FindIndex(MatchSlots, slot => slot.UserID == -1 
                                                                   && slot.SlotStatus != SlotStatus.Locked);

            if (newSlotIndex == -1)
                return null;

            MatchSlots[newSlotIndex].SetUser(userId);
  
            return (uint)newSlotIndex;
        }

        public bool Start(int slotId)
        {
            if (MatchSlots[slotId].UserID != HostID)
                return false;
            
            Array.ForEach(MatchSlots, slot =>
            {
                if (slot.SlotStatus == SlotStatus.Ready)
                    slot.SlotStatus = SlotStatus.Playing;
            });

            return true;
        }
        
        public bool SetSlotStatus(int slotId, SlotStatus status)
        {
            var slot = MatchSlots[slotId];
            
            if (slot.UserID == -1)
                return false;
            
            MatchSlots[slotId].SlotStatus = status;

            return true;
        }

        public bool MoveSlot(int slotId, int newSlotId)
        {
            if (slotId == newSlotId)
                return false;
            
            MatchSlots[slotId].Move(ref MatchSlots[newSlotId]);

            return true;
        }

        public (bool, bool) Leave(int slotId)
        {
            var slot = MatchSlots[slotId];
            if (slot.UserID == -1)
                return (false, false);

            MatchSlots[slotId].Reset();

            var newHost = Array.FindIndex(MatchSlots, s => s.UserID != -1);
            if (newHost == -1)
                return (true, true);
            
            HostID = MatchSlots[newHost].UserID;
            
            return (true, false);
        }

        public bool NoBeatmap(int slotId)
        {
            var slot = MatchSlots[slotId];
            
            if (slot.UserID == -1)
                return false;

            MatchSlots[slotId].SlotStatus |= SlotStatus.NoMap;

            return true;
        }
        
        public bool GotBeatmap(int slotId)
        {
            var slot = MatchSlots[slotId];
            
            if (slot.UserID == -1)
                return false;

            MatchSlots[slotId].SlotStatus &= ~SlotStatus.NoMap;

            return true;
        }
        
        public bool ChangeMods(int slotId, short mods)
        {
            if (MatchSlots[slotId].UserID != HostID)
                return false;

            Settings.ActiveMods = mods;

            return true;
        }

        public bool LockSlot(int slotId, int lockedSlot)
        {
            if (MatchSlots[slotId].UserID != HostID)
                return false;

            if (slotId == lockedSlot)
                return false;

            MatchSlots[lockedSlot].ToggleLock();

            return true;
        }

        public object Clone()
        {
            var match = (MemberwiseClone() as MatchState)!;
            match.Settings = (match.Settings.Clone() as MatchSettings)!;
            
            return match;
        }
    }
}