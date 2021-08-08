using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Oldsu.Bancho.Exceptions.Match;
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
        public bool AllCompleted => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Completed);
        public bool AllLoaded => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Loaded);
        public bool AllSkipped => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Skipped);

        private void UpdateSupportedVersions()
        {
            // Used for compatibility with future versions
        }

        public bool ChangeSettings(uint userId, MatchSettings settings)
        {
            if (userId != HostID)
                return false;

            Settings = settings;

            switch (settings.TeamType)
            {
                case MatchTeamTypes.HeadToHead:
                case MatchTeamTypes.TagCoop:
                    Array.ForEach(MatchSlots, slots => slots.SlotTeam = SlotTeams.Neutral);
                    break;
                case MatchTeamTypes.TeamVs:
                case MatchTeamTypes.TagTeamVs:
                    Array.ForEach(MatchSlots, slots => slots.SlotTeam = SlotTeams.Blue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            UpdateSupportedVersions();

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

            for (int i = 0; i < MaxMatchSize; i++)
                MatchSlots[i] = new MatchSlot();
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

        public void CheckHost(uint requesterUserId)
        {
            if (requesterUserId != HostID)
                throw new UserNotHostException();
        }

        public uint[] GetPlayingUsersIDs()
        {
            return MatchSlots.Where(slot => (slot.SlotStatus & SlotStatus.Playing) > 0)
                .Select(slot => (uint) slot.UserID).ToArray();
        }
        
        public MatchSlot GetSlotByPlayerID(uint requesterUserId)
        {            
            var index = Array.FindIndex(MatchSlots, slot => slot.UserID == requesterUserId);
            return MatchSlots[index];
        }
        
        public uint GetSlotIndexByPlayerID(uint requesterUserId)
        {            
            var index = Array.FindIndex(MatchSlots, slot => slot.UserID == requesterUserId);
            return (uint)index;
        }
        
        public void Start(uint requesterSlotId)
        {
            CheckHost(requesterSlotId);

            InProgress = true;
            
            Array.ForEach(MatchSlots, slot =>
            {
                if (slot.SlotStatus == SlotStatus.Ready)
                    slot.SlotStatus = SlotStatus.Playing;
            });
        }
        
        public bool MoveSlot(uint requesterUserId, uint newSlotId)
        {
            if (newSlotId >= 8)
                throw new InvalidSlotIDException();
            
            var currentSlot = GetSlotByPlayerID(requesterUserId);
            var newSlot = MatchSlots[newSlotId];

            if (currentSlot.UserID == newSlot.UserID)
                return false;
            
            currentSlot.Move(newSlot);

            return true;
        }

        public void Leave(uint requesterUserId, out bool disbandRequested)
        {
            disbandRequested = false;

            var slot = GetSlotByPlayerID(requesterUserId);
            
            slot.Reset();

            var newHost = Array.FindIndex(MatchSlots, s => s.UserID != -1);
            if (newHost == -1)
            {
                disbandRequested = true;
                return;
            }

            if (InProgress && GetPlayingUsersIDs().Length == 0)
            {
                Reset();
            }
            
            HostID = MatchSlots[newHost].UserID;
        }

        public void NoBeatmap(uint requesterUserId)
        {
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.SlotStatus |= SlotStatus.NoMap;
        }
        
        public void GotBeatmap(uint requesterUserId)
        {
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.SlotStatus &= ~SlotStatus.NoMap;
        }
        
        public void ChangeMods(uint requesterUserId, short mods)
        {
            CheckHost(requesterUserId);
            Settings.ActiveMods = mods;
        }

        public bool LockSlot(uint requesterUserId, uint lockedSlot, out uint? kick)
        {
            CheckHost(requesterUserId);
            kick = null;
            
            if (GetSlotIndexByPlayerID(requesterUserId) == lockedSlot)
                return false;

            if (lockedSlot >= 8)
                throw new InvalidSlotIDException();

            var slot = MatchSlots[lockedSlot];

            if (slot.UserID != -1)
                kick = (uint)slot.UserID;
            
            slot.ToggleLock();

            return true;
        }

        public void Skip(uint requesterUserId)
        {
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.Skipped = true;
        }
        
        public void Complete(uint requesterUserId)
        {            
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.Completed = true;
        }

        public void Reset()
        {
            InProgress = false;
            
            Array.ForEach(MatchSlots, slot =>
            {
                if (slot.SlotStatus != SlotStatus.Playing) 
                    return;
                
                slot.SlotStatus = SlotStatus.NotReady;
                slot.Completed = false;
                slot.Loaded = false;
                slot.Skipped = false;
            });
        }

        public void Load(uint requesterUserId)
        {            
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.Loaded = true;
        }
        
        
        public void Ready(uint requesterUserId)
        {
            var slot = GetSlotByPlayerID(requesterUserId);
            slot.SlotStatus = SlotStatus.Ready;
        }
        
        public void Unready(uint requesterSlotId)
        {
            var slot = GetSlotByPlayerID(requesterSlotId);
            slot.SlotStatus = SlotStatus.NotReady;
        }
        
        public object Clone()
        {
            var match = (MemberwiseClone() as MatchState)!;
            match.Settings = (match.Settings.Clone() as MatchSettings)!;
            
            return match;
        }

        public void ChangeTeam(uint requesterSlotId)
        {
            var slot = GetSlotByPlayerID(requesterSlotId);

            if (Settings.TeamType == MatchTeamTypes.HeadToHead)
                throw new InvalidTeamTypeException();
            
            slot.SlotTeam = slot.SlotTeam == SlotTeams.Blue ? SlotTeams.Red : SlotTeams.Blue;
        }
    }
}