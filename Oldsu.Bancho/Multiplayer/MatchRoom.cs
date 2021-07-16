using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Bancho.Multiplayer.Objects;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Multiplayer
{
    public class Match
    {
        public const int MaxMatchSize = 8;
        
        public byte MatchID { get; set; }
        public int HostID { get; set; }
        
        public string GameName { get; set; }
        public string GamePassword { get; set; }
        public Version AllowedVersions { get; set; } // <- WTF

        public string BeatmapName { get; set; }
        public int BeatmapID { get; set; }
        public string BeatmapChecksum { get; set; }
        public Mode PlayMode { get; set; }
        public MatchScoringTypes ScoringType { get; set; }
        public MatchTeamTypes TeamType { get; set; }
        
        public bool InProgress { get; set; }
        public MatchType MatchType { get; set; }
        public short ActiveMods { get; set; }

        public MatchSlot[] MatchSlots { get; set; } = new MatchSlot[MaxMatchSize];
        private readonly ReaderWriterLockSlim _rwLock = new();
        
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

        public bool TryJoin(Client client, string? password)
        {
            throw new NotImplementedException();
            
            _rwLock.EnterWriteLock();

            bool joinSuccessful = false;
            
            try
            {
                for (int x = 0; x < MaxMatchSize; x++)
                {
                    if (MatchSlots[x].Client == null)
                    {
                        MatchSlots[x].SlotStatus = SlotStatus.NotReady;
                        MatchSlots[x].SlotTeam = TeamType is MatchTeamTypes.TeamVs or MatchTeamTypes.TagTeamVs ? SlotTeams.Blue : SlotTeams.Red;
                        MatchSlots[x].Client = client;

                        joinSuccessful = true;
                    }
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            // todo send packets too
            
            return joinSuccessful;
        }

        public void Leave(int clientId)
        {
            throw new NotImplementedException();
            
            foreach (var slot in MatchSlots.Where(slot => slot.Client != null))
                if (slot.Client.ClientContext.User.UserID == clientId)
                {
                    slot.Reset();
                }

            // todo send correct packets
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void MoveSlot()
        {
            throw new NotImplementedException();
        }
    }
}