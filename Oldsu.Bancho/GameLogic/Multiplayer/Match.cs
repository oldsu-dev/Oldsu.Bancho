using System;
using System.Collections.Generic;
using System.Linq;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Exceptions.Match;
using Oldsu.Bancho.GameLogic.Multiplayer.Enums;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Logging;
using Action = System.Action;
using ScoreFrame = Oldsu.Bancho.Objects.ScoreFrame;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.GameLogic.Multiplayer
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
    
    public class Match
    {
        public const int MaxSlots = 8;

        public int MatchID { get; set; }
        public int HostID { get; set; }
        
        public HashSet<Version> AllowedVersions { get; }

        public bool InProgress { get; set; }

        public event Action<Match>? OnUpdate;
        public event Action<Match>? OnDisband;
        
        public MatchSettings Settings { get; private set; }
        public MatchSlot[] MatchSlots { get; }
        
        public bool IsEmpty => MatchSlots.All(slot => slot.UserID == -1);
        public bool AllCompleted => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Completed);
        public bool AllLoaded => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Loaded);
        public bool AllSkipped => MatchSlots.All(slot => (slot.SlotStatus & SlotStatus.Playing) == 0 || slot.Skipped);
        
        private readonly LoggingManager _loggingManager;

        public Match(int matchId, User host, MatchSettings settings, LoggingManager loggingManager)
        {
            AllowedVersions = new HashSet<Version> { Version.B904 };

            _loggingManager = loggingManager;
            
            Settings = settings;
            UpdateSupportedVersions();
            
            MatchSlots = new MatchSlot[MaxSlots];

            MatchID = matchId;
            HostID = (int)host.UserID;

            for (int i = 0; i < MaxSlots; i++)
                MatchSlots[i] = new MatchSlot();

            TryJoin(host, settings.GamePassword!);

        }

        private void BroadcastToPlayers(SharedPacketOut packet)
        {
            CachedBanchoPacket cachedPacket = new CachedBanchoPacket(packet);
            
            for (int i = 0; i < MaxSlots; i++)
                MatchSlots[i].User?.SendPacket(cachedPacket);
            
            #region Logging

            #endregion
        }
        
        private void BroadcastToPlayersBut(SharedPacketOut packet, Predicate<MatchSlot> predicate)
        {
            CachedBanchoPacket cachedPacket = new CachedBanchoPacket(packet);

            for (int i = 0; i < MaxSlots; i++)
            {
                MatchSlot slot = MatchSlots[i];
                
                if (slot.User == null)
                    continue;

                if (predicate(slot))
                    continue;
                
                MatchSlots[i].User?.SendPacket(cachedPacket);
            }
        }

        private void NotifyUpdate()
        {
            BroadcastToPlayers(new MatchUpdate{Match = this});
            
            OnUpdate?.Invoke(this);
        }

        private void UpdateSupportedVersions()
        {
            // Used for compatibility with future versions
        }

        public void ChangeSettings(User user, MatchSettings settings)
        {
            AssertHost(user);

            if (InProgress)
                throw new UserPlayingException();
            
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
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Changed settings",
                dump: new { user.UserID, settings });
            
            #endregion
            
            NotifyUpdate();
        }

        public void TryJoin(User user, string password) 
        {
            if (password != Settings.GamePassword)
            {
                user.SendPacket(new MatchJoinFail());
                return;
            }

            var newSlotIndex = Array.FindIndex(MatchSlots, slot => slot.UserID == -1 
                                                                   && slot.SlotStatus != SlotStatus.Locked);
            if (newSlotIndex == -1)
            {
                user.SendPacket(new MatchJoinFail());
                return;
            }

            NotifyUpdate();
            
            MatchSlots[newSlotIndex].SetUser(user);
            user.Match = this;
            
            user.SendPacket(new MatchJoinSuccess{Match = this});
            user.SendPacket(new ChannelAvailable{ChannelName = "#multiplayer"});
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "User joined",
                dump: new { user.UserID, MatchID });

            #endregion
        }

        public void AssertHost(User user)
        {
            if (user.UserID != HostID)
                throw new UserNotHostException();
        }

        public uint[] GetPlayingUsersIDs()
        {
            return MatchSlots.Where(slot => (slot.SlotStatus & SlotStatus.Playing) > 0)
                .Select(slot => (uint) slot.UserID).ToArray();
        }
        
        public void AssertPlayerInMatch(User user)
        {            
            var index = Array.FindIndex(MatchSlots, slot => slot.UserID == user.UserID);

            if (index == -1)
                throw new UserNotInMatchException();
        }

        
        public MatchSlot GetSlotByPlayer(User user)
        {            
            var index = Array.FindIndex(MatchSlots, slot => slot.UserID == user.UserID);

            if (index == -1)
                throw new UserNotInMatchException();
            
            return MatchSlots[index];
        }
        
        public uint GetSlotIndexByPlayer(User user)
        {            
            var index = Array.FindIndex(MatchSlots, slot => slot.UserID == user.UserID);

            if (index == -1)
                throw new UserNotInMatchException();            
            
            return (uint)index;
        }

        public void SendMessage(User sender, string contents)
        {
            AssertPlayerInMatch(sender);
            
            #region Logging

            _loggingManager.LogInfoSync<Match>(
                "Sent message in match",
                dump: new { sender.UserID, Contents = contents, MatchID });
            
            #endregion
            
            BroadcastToPlayersBut(
                new SendMessage{Contents = contents, Sender = sender.Username, Target = "#multiplayer"}, 
                slot => slot.UserID == sender.UserID);
        }
        
        public void Start(User invoker)
        {
            AssertHost(invoker);

            if (InProgress)
                throw new UserPlayingException();
            
            InProgress = true;

            Array.ForEach(MatchSlots, slot =>
            {
                if (slot.SlotStatus == SlotStatus.Ready)
                    slot.SlotStatus = SlotStatus.Playing;
            });
            
            #region Logging

            _loggingManager.LogInfoSync<Match>(
                "Match started",
                dump: new { invoker.UserID });
            
            #endregion
            
            NotifyUpdate();
            BroadcastToPlayersBut(new MatchStart{Match = this}, slot => slot.SlotStatus != SlotStatus.Playing);
        }
        
        public void MoveSlot(User invoker, uint newSlotId)
        {
            if (newSlotId >= 8)
                throw new InvalidSlotIDException();
            
            var currentSlotIndex = GetSlotIndexByPlayer(invoker);

            var currentSlot = MatchSlots[currentSlotIndex];
            var newSlot = MatchSlots[newSlotId];

            if (newSlot.SlotStatus == SlotStatus.Locked)
                return;
            
            if (currentSlot.UserID == newSlot.UserID)
                return;
         
            currentSlot.Move(newSlot);
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Move to new slot",
                dump: new { invoker.UserID, CurrentSlotIndex = currentSlot, NewSlotIndex = newSlotId });
            
            #endregion
            
            NotifyUpdate();
        }

        public void Leave(User invoker, bool updateSlot = true)
        {
            if (updateSlot)
            {
                var slot = GetSlotByPlayer(invoker);
                slot.Reset();
            }

            if (invoker.UserID == HostID)
            {
                var newHost = Array.FindIndex(MatchSlots, s => s.UserID != -1);
                if (newHost == -1)
                {
                    OnDisband?.Invoke(this);
                }
                else
                {
                    if (InProgress && GetPlayingUsersIDs().Length == 0)
                    {
                        Reset();
                    }

                    HostID = MatchSlots[newHost].UserID;
                }
            }

            invoker.SendPacket(new ChannelLeft{ChannelName = "#multiplayer"});
            invoker.Match = null;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Move to new slot",
                dump: new { invoker.UserID });
            
            #endregion

            
            NotifyUpdate();
        }

        public void NoBeatmap(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);

            if (slot.SlotStatus == SlotStatus.Playing)
                throw new UserPlayingException();
            
            slot.SlotStatus = SlotStatus.NoMap;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "No beatmap",
                dump: new { invoker.UserID });
            
            #endregion
            
            NotifyUpdate();
        }
        
        public void GotBeatmap(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);
            
            if (slot.SlotStatus == SlotStatus.Playing)
                throw new UserPlayingException();
            
            slot.SlotStatus = SlotStatus.NotReady;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Got beatmap",
                dump: new { invoker.UserID });
            
            #endregion
            
            NotifyUpdate();
        }
        
        public void ChangeMods(User invoker, short mods)
        {
            if (InProgress)
                throw new UserPlayingException();

            AssertHost(invoker);
            Settings.ActiveMods = mods;

            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Change mods",
                dump: new { invoker.UserID, Mods = mods });
            
            #endregion
            
            NotifyUpdate();
        }

        public void LockSlot(User invoker, uint lockedSlot)
        {
            AssertHost(invoker);
            
            if (lockedSlot >= 8)
                throw new InvalidSlotIDException();
            
            if (GetSlotIndexByPlayer(invoker) == lockedSlot)
                return;

            var slot = MatchSlots[lockedSlot];

            if (slot.User != null)
                Leave(slot.User, updateSlot: false);
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Slot locked",
                dump: new { invoker.UserID, SlotIndex = lockedSlot });
            
            #endregion
            
            slot.ToggleLock();
            
            NotifyUpdate();
        }
        
        public void TransferHost(User invoker, uint newHostSlot)
        {
            AssertHost(invoker);
            
            if (newHostSlot >= 8)
                throw new InvalidSlotIDException();
            
            if (GetSlotIndexByPlayer(invoker) == newHostSlot)
                return;

            var slot = MatchSlots[newHostSlot];
            
            if (slot.User == null)
                throw new InvalidSlotIDException();

            HostID = slot.UserID;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Host transferred",
                dump: new { invoker.UserID, NewHost = HostID });
            
            #endregion
            
            NotifyUpdate();
        }

        public void Skip(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);

            if (slot.Skipped)
                throw new UserAlreadySkippedException();

            slot.Skipped = true;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Skipped intro",
                dump: new { invoker.UserID });
            
            #endregion

            if (AllSkipped)
            {
                #region Logging
            
                _loggingManager.LogInfoSync<Match>("Everyone skipped the intro");
            
                #endregion
                
                BroadcastToPlayers(new MatchSkip());
            }
        }
        
        public void Complete(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);

            if (slot.Completed)
                throw new UserAlreadyCompletedException();

            slot.Completed = true;

            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Completed the beatmap",
                dump: new { invoker.UserID });
            
            #endregion
            
            if (AllCompleted)
            {
                #region Logging
            
                _loggingManager.LogInfoSync<Match>(
                    "The match is ended",
                    dump: new { FinalResult = MatchSlots
                        .Where(slot => slot.LastScoreFrame != null)
                        .Select(slot => new { slot.UserID, slot.LastScoreFrame })
                        .OrderByDescending(frame => frame.LastScoreFrame!.Score).ToArray() 
                    });
            
                #endregion
                
                BroadcastToPlayers(new MatchComplete());
                Reset();
            }
        }

        public void Load(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);

            if (slot.Loaded)
                throw new UserAlreadyLoadedException();

            slot.Loaded = true;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Loaded beatmap",
                dump: new { invoker.UserID });
            
            #endregion

            if (AllLoaded)
            {
                #region Logging
            
                _loggingManager.LogInfoSync<Match>(
                    "Everyone loaded the beatmap",
                    dump: new { invoker.UserID });
            
                #endregion
                
                BroadcastToPlayers(new MatchLoad());
            }
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
            
            NotifyUpdate();
        }

        public void Ready(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);
            
            if (slot.SlotStatus == SlotStatus.Playing)
                throw new UserPlayingException();
            
            slot.SlotStatus = SlotStatus.Ready;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Ready",
                dump: new { invoker.UserID });
            
            #endregion
            
            NotifyUpdate();
        }

        public void Unready(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);
            
            if (slot.SlotStatus == SlotStatus.Playing)
                throw new UserPlayingException();
            
            slot.SlotStatus = SlotStatus.NotReady;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Unready",
                dump: new { invoker.UserID });
            
            #endregion
            
            NotifyUpdate();
        }
        
        public void ChangeTeam(User invoker)
        {
            var slot = GetSlotByPlayer(invoker);

            if (Settings.TeamType == MatchTeamTypes.HeadToHead)
                throw new InvalidTeamTypeException();

            slot.SlotTeam = slot.SlotTeam == SlotTeams.Blue ? SlotTeams.Red : SlotTeams.Blue;
            
            #region Logging
            
            _loggingManager.LogInfoSync<Match>(
                "Changed team",
                dump: new { invoker.UserID, slot.SlotTeam });
            
            #endregion
            
            NotifyUpdate();
        }

        public void ScoreUpdate(User invoker, ScoreFrame scoreFrame)
        {
            uint slotIndex = GetSlotIndexByPlayer(invoker);
            MatchSlot slot = MatchSlots[slotIndex];

            scoreFrame.SlotID = (byte)slotIndex;
            slot.LastScoreFrame = scoreFrame;

            BroadcastToPlayers(new MatchScoreUpdate{ScoreFrame = scoreFrame});
        }
    }
}