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

        private MatchSlot[] MatchSlots { get; set; } = new MatchSlot[MaxMatchSize];
        private readonly ReaderWriterLockSlim _rwLock = new();

        public RwLockableEnumerable<MatchSlot> Slots => new(_rwLock, MatchSlots);

        public int SkipRequests = 0;
        
        public Match(string gameName, string gamePassword, int beatmapId, 
            string? beatmapName, string? beatmapChecksum)
        {
            GameName = gameName;
            GamePassword = gamePassword;
            BeatmapName = beatmapName;
            BeatmapChecksum = beatmapChecksum;
            BeatmapID = beatmapId;
            
            // b394 lacks password field in bMatch, so it wont be in the lobby.
            if (GamePassword is not (null or ""))
                AllowedVersions.Remove(Version.B394A);
            
            Array.Fill(MatchSlots, 
                new MatchSlot { Client = null, SlotStatus = SlotStatus.Open, SlotTeam = SlotTeams.Neutral});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="password"></param>
        /// <returns>Slot ID</returns>
        public int? TryJoin(Client client, string? password)
        {
            _rwLock.EnterWriteLock();

            try
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

                    return i;
                }
                
                return null;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Leave(int slotId)
        {
            _rwLock.EnterWriteLock();
            
            try
            {
                MatchSlots[slotId].Reset();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            // todo send correct packets
        }

        public void Start()
        {
            _rwLock.EnterWriteLock();

            try
            {
                Array.ForEach(MatchSlots, slot =>
                {
                    slot.SlotStatus = SlotStatus.Playing;
                    slot.Completed = false;
                    slot.Loaded = false;
                    slot.Skipped = false;
                });
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void ForceStop()
        {            
            _rwLock.EnterWriteLock();

            try
            {
                for (int i = 0; i < MaxMatchSize; i++)
                    Complete(i);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void UnreadyAll()
        {
            _rwLock.EnterWriteLock();

            try
            {
                Array.ForEach(MatchSlots, slot => slot.SlotStatus = SlotStatus.NotReady);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Wether is skipped by all the users or not.</returns>
        public bool Skip(int slotId)
        {
            _rwLock.EnterWriteLock();

            try
            {
                MatchSlots[slotId].Skipped = true;
                return MatchSlots.All(slot => slot.Skipped);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool CompleteLoad(int slotId)
        {
            _rwLock.EnterWriteLock();

            try
            {
                MatchSlots[slotId].Loaded = true;
                return MatchSlots.All(slot => slot.Loaded);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        
        public bool Complete(int slotId)
        {
            _rwLock.EnterWriteLock();

            try
            {
                MatchSlots[slotId].Completed = true;
                return MatchSlots.All(slot => slot.Completed);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool MoveSlot(int currentSlot, int newSlot)
        {
            _rwLock.EnterWriteLock();

            try
            {
                if (MatchSlots[newSlot].Client != null)
                    return false;

                MatchSlots[currentSlot].Move(ref MatchSlots[newSlot]);
                return true;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool TransferHost(int currentSlot, int newSlot)
        {
            //todo
            throw new NotImplementedException();
        }
    }
}