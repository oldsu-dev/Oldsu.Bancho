using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B904;
using Oldsu.Multiplayer.Enums;
using Oldsu.Utils.Threading;
using MatchDisband = Oldsu.Bancho.Packet.Shared.Out.MatchDisband;
using MatchUpdate = Oldsu.Bancho.Packet.Shared.Out.MatchUpdate;

namespace Oldsu.Bancho.Providers.InMemory
{
    // This need some future refactoring
    
    public class InMemoryMatchObservable : InMemoryObservable<ProviderEvent>, IMatchObservable { }
    
    public class InMemoryLobbyProvider : InMemoryObservable<ProviderEvent>, ILobbyProvider
    {
        private const int MatchesAvailable = 256;

        public InMemoryLobbyProvider()
        {
            _matches = new AsyncRwLockWrapper<MatchState?[]>(new MatchState?[MatchesAvailable]);
            _joinedPlayers = new AsyncRwLockWrapper<Dictionary<uint, JoinedPlayerData>>(new());
            _inGamePlayers = new AsyncRwLockWrapper<Dictionary<uint, InGamePlayerData>>(new());
            
            _matchObservables =
                new AsyncRwLockWrapper<InMemoryMatchObservable?[]>(new InMemoryMatchObservable[MatchesAvailable]);
        }

        struct JoinedPlayerData
        {
            public uint MatchID { get; set; }
            public uint SlotID { get; set; }
        }
        
        struct InGamePlayerData
        {
            public uint MatchID { get; set; }
            public uint SlotID { get; set; }
            
            public AsyncMutexWrapper<ScoreFrame?> ScoreFrame { get; set; }
            public AsyncMutexWrapper<bool> CanSkip { get; set; }
            public AsyncMutexWrapper<bool> CanLoad { get; set; }
            public AsyncMutexWrapper<bool> CanComplete { get; set; }
        }
        
        private readonly AsyncRwLockWrapper<MatchState?[]> _matches;
        private readonly AsyncRwLockWrapper<InMemoryMatchObservable?[]> _matchObservables;
        private readonly AsyncRwLockWrapper<Dictionary<uint, JoinedPlayerData>> _joinedPlayers;
        private readonly AsyncRwLockWrapper<Dictionary<uint, InGamePlayerData>> _inGamePlayers;

        private async Task NotifyMatchUpdate(MatchState data)
        {
            // Hide original password before notify.
            if (data.Settings.GamePassword != null)
                data.Settings.GamePassword = "HasPassword";
            
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new MatchUpdate {MatchState = data}),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[data.MatchID]?.Notify(ev) ?? Task.CompletedTask));
            
            await Notify(ev);
        }


        private async Task NotifyMatchDisband(uint matchId)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new MatchDisband() {MatchID = (int)matchId}),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
            
            await Notify(ev);
        }
        
        private async Task NotifyMatchStart(MatchState data)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchStart {MatchState = data}),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[data.MatchID]?.Notify(ev) ?? Task.CompletedTask));
        }

        private async Task NotifyMatchScore(uint matchId, ScoreFrame scoreFrame)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchScoreUpdate {ScoreFrame = scoreFrame}),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
        }

        private async Task NotifyMatchSkip(uint matchId)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchSkip()),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
        }
        
        private async Task NotifyMatchLoad(uint matchId)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchLoad()),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
        }

        private async Task NotifyMatchComplete(uint matchId)
        {
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchComplete()),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
        }
        
        public async Task<MatchState?> GetMatchState(uint matchId)
        {
            using var matchesLock = await _matches.AcquireReadLockGuard();
            return (MatchState?)(-matchesLock)[matchId]?.Clone() ?? null;
        }

        public async Task<IMatchObservable?> MatchGetObservable(uint userId)
        {
            using var pairsLock = await _joinedPlayers.AcquireReadLockGuard();
            using var observablesLock = await _matchObservables.AcquireReadLockGuard();
            
            if (!(-pairsLock).TryGetValue(userId, out var pair))
                return null;

            return (-observablesLock)[pair.MatchID];
        }

        public async Task<MatchState?> CreateMatch(uint userId, MatchSettings matchSettings)
        {
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            for (uint i = 0; i < MatchesAvailable; i++)
            {
                if ((-matchesLock)[i] != null) 
                    continue;
                
                var match = (-matchesLock)[i] = new MatchState((int)i, (int)userId, matchSettings);
                
                var newSlot = match.Join((int)userId, matchSettings.GamePassword!);
                
                await _joinedPlayers.WriteAsync(pairs => 
                    pairs.Add(userId, new JoinedPlayerData{ MatchID = i, SlotID = newSlot!.Value}));

                await _matchObservables.WriteAsync(observables => 
                    observables[i] = new InMemoryMatchObservable()); 
                
                await NotifyMatchUpdate((match.Clone() as MatchState)!);
                return match;
            }
            
            return null;
        }

        public Task<MatchState[]> GetAvailableMatches() =>
            _matches.ReadAsync(matches => matches.Where(m => m != null)
                .Select(m => (MatchState)m!.Clone()).ToArray())!;
        
        public async Task<MatchState?> JoinMatch(uint userId, uint matchId, string password)
        {
            if (await _joinedPlayers.ReadAsync(pairs => pairs.ContainsKey(userId)))
                return null;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            var matchArrayLength = (-matchesLock).Length;

            if (matchId >= matchArrayLength)
                return null;

            if ((-matchesLock)[matchId] == null)
                return null;
            
            var match = (-matchesLock)[matchId];
            
            var newSlot = match!.Join((int)userId, password);
            if (newSlot is null)
                return null;

            await _joinedPlayers.WriteAsync(pairs => 
                pairs.Add(userId, new JoinedPlayerData{ MatchID = matchId, SlotID = newSlot!.Value}));

            await NotifyMatchUpdate((match.Clone() as MatchState)!);
            
            return (MatchState)match.Clone();
        }
        
        public async Task<bool> LeaveMatch(uint userId)
        {
            JoinedPlayerData playerData = default;
            if (!await _joinedPlayers.ReadAsync(pairs => pairs.TryGetValue(userId, out playerData)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            var match = (-matchesLock)[playerData.MatchID]!;
            
            var (success, disbandMatch) = match.Leave((int)playerData.SlotID);
            if (!success)
                return false;
            
            await _joinedPlayers.WriteAsync(players => players.Remove(userId));
            await _inGamePlayers.WriteAsync(players => players.Remove(userId));

            if (disbandMatch)
            {
                (-matchesLock)[playerData.MatchID] = null;
                await NotifyMatchDisband(playerData.MatchID);
            } else
                await NotifyMatchUpdate((match.Clone() as MatchState)!);

            return true;
        }
        
        private async Task<bool> ExecuteOperationOnCurrentMatch(uint userId, Func<MatchState, int, bool> fn)
        {
            JoinedPlayerData playerData = default;
            if (!await _joinedPlayers.ReadAsync(pairs => pairs.TryGetValue(userId, out playerData)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            
            bool success = fn((-matchesLock)[playerData.MatchID]!, (int)playerData.SlotID);

            if (success)
                await NotifyMatchUpdate(((-matchesLock)[playerData.MatchID]!.Clone() as MatchState)!);

            return success;
        }
        
        private async Task<bool> ExecuteOperationOnCurrentMatch(uint userId, Func<MatchState, int, Task<bool>> fn)
        {
            JoinedPlayerData playerData = default;
            if (!await _joinedPlayers.ReadAsync(pairs => pairs.TryGetValue(userId, out playerData)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            bool success = await fn((-matchesLock)[playerData.MatchID]!, (int)playerData.SlotID);

            if (success)
                await NotifyMatchUpdate(((-matchesLock)[playerData.MatchID]!.Clone() as MatchState)!);

            return success;
        }

        public async Task<uint?> GetCurrentMatch(uint userId)
        {
            using var pairsLock = await _joinedPlayers.AcquireReadLockGuard();
            
            if (!(-pairsLock).TryGetValue(userId, out var pair))
                return null;

            return pair.MatchID;
        }
        
        public Task<bool> MatchSetReady(uint userId) => 
            ExecuteOperationOnCurrentMatch(userId, 
                (match, slotId) => match.SetSlotStatus(slotId, SlotStatus.Ready));

        public Task<bool> MatchGotBeatmap(uint userId) =>
            ExecuteOperationOnCurrentMatch(userId, (match, slotId) => match.GotBeatmap(slotId));

        public Task<bool> MatchSetUnready(uint userId) => 
            ExecuteOperationOnCurrentMatch(userId, 
                (match, slotId) => match.SetSlotStatus(slotId, SlotStatus.NotReady));
        
        public Task<bool> MatchMoveSlot(uint userId, int newSlot) => 
            ExecuteOperationOnCurrentMatch(userId, 
                async (match, slotId) =>
                {
                    if (!match.MoveSlot(slotId, newSlot)) 
                        return false;
                    
                    await _joinedPlayers.WriteAsync(pairs =>
                        pairs[userId] = new JoinedPlayerData{MatchID = (uint) match.MatchID, SlotID = (uint)newSlot});

                    return true;

                });
        
        public Task<bool> MatchChangeSettings(uint userId, MatchSettings matchSettings) => 
            ExecuteOperationOnCurrentMatch(userId, 
                (match, slotId) => match.ChangeSettings(slotId, matchSettings));

        public Task<bool> MatchNoBeatmap(uint userId) =>
            ExecuteOperationOnCurrentMatch(userId, (match, slotId) => match.NoBeatmap(slotId));

        public Task<bool> MatchChangeMods(uint userId, short mods) =>
            ExecuteOperationOnCurrentMatch(userId, (match, slotId) => match.ChangeMods(slotId, mods));
        
        public Task<bool> MatchLockSlot(uint userId, uint slot) =>
            ExecuteOperationOnCurrentMatch(userId, (match, slotId) => match.LockSlot(slotId, (int)slot));

        public async Task<bool> MatchStart(uint userId)
        {
            JoinedPlayerData playerData = default;
            if (!await _joinedPlayers.ReadAsync(pairs => pairs.TryGetValue(userId, out playerData)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            var match = (-matchesLock)[playerData.MatchID]!;
            
            if (!match.Start((int)playerData.SlotID))
                return false;

            var inMatchPlayers = match.MatchSlots
                .Where((slot) => slot.UserID != -1)
                .Select((slot, i) => new
                {
                    SlotID = i,
                    slot.UserID
                });

            await _inGamePlayers.WriteAsync(players =>
            {
                foreach (var player in inMatchPlayers)
                    players.Add((uint) player.UserID, new InGamePlayerData
                    {
                        CanComplete = new AsyncMutexWrapper<bool>(true),
                        CanSkip = new AsyncMutexWrapper<bool>(true),
                        CanLoad = new AsyncMutexWrapper<bool>(true),
                        ScoreFrame = new AsyncMutexWrapper<ScoreFrame?>(null),
                        MatchID = (uint) match.MatchID,
                        SlotID = (uint) player.SlotID
                    });

            });

            var notifyMatchData = (match.Clone() as MatchState)!;

            await NotifyMatchUpdate(notifyMatchData);
            await NotifyMatchStart(notifyMatchData);

            return true;
        }
        
        public async Task<bool> MatchScoreUpdate(uint userId, ScoreFrame scoreFrame)
        {
            using var inGamePlayersLock = await _inGamePlayers.AcquireReadLockGuard();

            if (!(-inGamePlayersLock).TryGetValue(userId, out var playerData))
                return false;

            scoreFrame.SlotID = (byte)playerData.SlotID;

            await playerData.ScoreFrame.SetValueAsync(scoreFrame);
            await NotifyMatchScore(playerData.MatchID, scoreFrame);

            return true;
        }
        
        public async Task<bool> MatchSkip(uint userId)
        {
            using var inGamePlayersLock = await _inGamePlayers.AcquireReadLockGuard();

            if (!(-inGamePlayersLock).TryGetValue(userId, out var playerData))
                return false;

            if (!await playerData.CanSkip.LockAsync(v => v))
                return false;

            await playerData.CanSkip.SetValueAsync(false);
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            var match = (-matchesLock)[playerData.MatchID]!;
            
            match.Skip(playerData.SlotID);

            if (match.AllSkipped)
                await NotifyMatchSkip((uint)match.MatchID);
            
            return true;
        }
        
        public async Task<bool> MatchLoad(uint userId)
        {
            using var inGamePlayersLock = await _inGamePlayers.AcquireReadLockGuard();
            
            if (!(-inGamePlayersLock).TryGetValue(userId, out var playerData))
                return false;

            if (!await playerData.CanLoad.LockAsync(v => v))
                return false;

            await playerData.CanLoad.SetValueAsync(false);
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            var match = (-matchesLock)[playerData.MatchID]!;
            
            match.Complete(playerData.SlotID);

            if (match.AllLoaded)
                await NotifyMatchLoad((uint)match.MatchID);
            
            return true;
        }
        
        public async Task<bool> MatchComplete(uint userId)
        {
            var inGamePlayersLock = await _inGamePlayers.AcquireReadLockGuard();
            
            if (!(-inGamePlayersLock).TryGetValue(userId, out var playerData))
                return false;

            if (!await playerData.CanComplete.LockAsync(v => v))
                return false;

            await playerData.CanComplete.SetValueAsync(false);
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            var match = (-matchesLock)[playerData.MatchID]!;
            
            match.Complete(playerData.SlotID);
            
            if (match.AllCompleted)
                await NotifyMatchComplete((uint)match.MatchID);

            inGamePlayersLock.Dispose();
            
            await _inGamePlayers.WriteAsync(players => players.Remove(userId));
            
            return true;
        }
    }
}