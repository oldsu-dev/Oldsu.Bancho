using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Out.B904;
using Oldsu.Multiplayer.Enums;
using Oldsu.Utils.Threading;
using MatchDisband = Oldsu.Bancho.Packet.Shared.Out.MatchDisband;
using MatchUpdate = Oldsu.Bancho.Packet.Shared.Out.MatchUpdate;

namespace Oldsu.Bancho.Providers.InMemory
{
    
    public class InMemoryMatchObservable : InMemoryObservable<ProviderEvent>, IMatchObservable { }
    
    public class InMemoryLobbyProvider : InMemoryObservable<ProviderEvent>, ILobbyProvider
    {
        private const int MatchesAvailable = 256;

        public InMemoryLobbyProvider()
        {
            _matches = new AsyncRwLockWrapper<MatchState?[]>(new MatchState?[MatchesAvailable]);
            _joinedMatchPairs = new AsyncRwLockWrapper<Dictionary<uint, (uint MatchID, uint SlotID)>>(new());
            
            _matchObservables =
                new AsyncRwLockWrapper<InMemoryMatchObservable?[]>(new InMemoryMatchObservable[MatchesAvailable]);
        }
        
        private readonly AsyncRwLockWrapper<MatchState?[]> _matches;
        private readonly AsyncRwLockWrapper<InMemoryMatchObservable?[]> _matchObservables;
        private readonly AsyncRwLockWrapper<Dictionary<uint, (uint MatchID, uint SlotID)>> _joinedMatchPairs;
        
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
            // Hide original password before notify.
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
        
        private async Task NotifyMatchStart(uint matchId)
        {
            // Hide original password before notify.
            var ev = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = new BanchoPacket(new Packet.Shared.Out.MatchStart()),
                DataType = ProviderEventType.BanchoPacket
            };
            
            await _matchObservables.ReadAsync(async observables =>
                await (observables[matchId]?.Notify(ev) ?? Task.CompletedTask));
            
            await Notify(ev);
        }
        
        public async Task<MatchState?> GetMatchState(uint matchId)
        {
            using var matchesLock = await _matches.AcquireReadLockGuard();
            return (MatchState?)(-matchesLock)[matchId]?.Clone() ?? null;
        }

        public async Task<IMatchObservable?> MatchGetObservable(uint userId)
        {
            using var pairsLock = await _joinedMatchPairs.AcquireReadLockGuard();
            using var observablesLock = await _matchObservables.AcquireReadLockGuard();
            
            if (!(-pairsLock).TryGetValue(userId, out var pair))
                return null;

            return (-observablesLock)[pair.MatchID];
        }

        public async Task<MatchState?> CreateMatch(uint userId, MatchSettings matchSettings)
        {
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            for (int i = 0; i < MatchesAvailable; i++)
            {
                if ((-matchesLock)[i] != null) 
                    continue;
                
                var match = (-matchesLock)[i] = new MatchState(i, (int)userId, matchSettings);
                
                var newSlot = match.Join((int)userId, matchSettings.GamePassword!);
                
                await _joinedMatchPairs.WriteAsync(pairs => 
                    pairs.Add(userId, ((uint) i, newSlot!.Value)));

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
            if (await _joinedMatchPairs.ReadAsync(pairs => pairs.ContainsKey(userId)))
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

            await _joinedMatchPairs.WriteAsync(pairs => 
                pairs.Add(userId, (matchId, newSlot.Value)));

            await NotifyMatchUpdate((match.Clone() as MatchState)!);
            
            return (MatchState)match.Clone();
        }
        
        public async Task<bool> LeaveMatch(uint userId)
        {
            (uint MatchID, uint SlotID) pair = default;
            if (!await _joinedMatchPairs.ReadAsync(pairs => pairs.TryGetValue(userId, out pair)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            var match = (-matchesLock)[pair.MatchID]!;
            
            var (success, disbandMatch) = match.Leave((int)pair.SlotID);
            if (!success)
                return false;
            
            await _joinedMatchPairs.WriteAsync(pairs => pairs.Remove(userId));

            if (disbandMatch)
            {
                (-matchesLock)[pair.MatchID] = null;
                await NotifyMatchDisband(pair.MatchID);
            } else
                await NotifyMatchUpdate((match.Clone() as MatchState)!);

            return true;
        }
        
        private async Task<bool> ExecuteOperationOnCurrentMatch(uint userId, Func<MatchState, int, bool> fn)
        {
            (uint MatchID, uint SlotID) pair = default;
            if (!await _joinedMatchPairs.ReadAsync(pairs => pairs.TryGetValue(userId, out pair)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();
            
            bool success = fn((-matchesLock)[pair.MatchID]!, (int)pair.SlotID);

            if (success)
                await NotifyMatchUpdate(((-matchesLock)[pair.MatchID]!.Clone() as MatchState)!);

            return success;
        }
        
        private async Task<bool> ExecuteOperationOnCurrentMatch(uint userId, Func<MatchState, int, Task<bool>> fn)
        {
            (uint MatchID, uint SlotID) pair = default;
            if (!await _joinedMatchPairs.ReadAsync(pairs => pairs.TryGetValue(userId, out pair)))
                return false;
            
            using var matchesLock = await _matches.AcquireWriteLockGuard();

            bool success = await fn((-matchesLock)[pair.MatchID]!, (int)pair.SlotID);

            if (success)
                await NotifyMatchUpdate(((-matchesLock)[pair.MatchID]!.Clone() as MatchState)!);

            return success;
        }

        public async Task<uint?> GetCurrentMatch(uint userId)
        {
            using var pairsLock = await _joinedMatchPairs.AcquireReadLockGuard();
            
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
                    
                    await _joinedMatchPairs.WriteAsync(pairs =>
                        pairs[userId] = ((uint) match.MatchID, (uint) newSlot));

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
        
        public Task<bool> MatchStart(uint userId) =>
            ExecuteOperationOnCurrentMatch(userId, async (match, slotId) =>
            {
                if (!match.Start(slotId)) 
                    return false;
               
                await NotifyMatchStart((uint)match.MatchID);
                return true;
            });
    }
}