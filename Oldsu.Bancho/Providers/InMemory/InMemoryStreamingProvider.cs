using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Logging;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryStreamerObservable : InMemoryObservable<ProviderEvent>, IStreamerObservable { }
    public class InMemorySpectatorObservable : InMemoryObservable<ProviderEvent>, ISpectatorObservable { }
    
    public class InMemoryStreamingProvider : IStreamingProvider
    {
        private readonly AsyncRwLockWrapper<Dictionary<uint, InMemoryStreamerObservable>> _streamerObservables;
        private readonly AsyncRwLockWrapper<Dictionary<uint, InMemorySpectatorObservable>> _spectatorObservables;
        private readonly AsyncMutexWrapper<Dictionary<uint, uint>> _streamingPairs;

        private readonly LoggingManager _loggingManager;
        
        public InMemoryStreamingProvider(LoggingManager loggingManager)
        {
            _streamerObservables = new AsyncRwLockWrapper<Dictionary<uint, InMemoryStreamerObservable>>(new());
            _spectatorObservables = new AsyncRwLockWrapper<Dictionary<uint, InMemorySpectatorObservable>>(new ());
            _streamingPairs = new AsyncMutexWrapper<Dictionary<uint, uint>>(new());

            _loggingManager = loggingManager;
        }

        public Task<IStreamerObservable?> GetStreamerObserver(uint userId) => 
            _streamerObservables.ReadAsync(
                observables =>
                {
                    if (!observables.TryGetValue(userId, out var observable))
                        return null;
                        
                    return (IStreamerObservable)observable;
                });

        public Task<ISpectatorObservable?> GetSpectatorObserver(uint userId) =>
            _spectatorObservables.ReadAsync(
                observables =>
                {
                    if (!observables.TryGetValue(userId, out var observable))
                        return null;
                    
                    return (ISpectatorObservable)observable;
                });

        public Task PushFrames(uint userId, byte[] frameData) =>
            _spectatorObservables.ReadAsync(observables => 
                observables[userId].Notify(new ProviderEvent
                {
                    ProviderType = ProviderType.Streaming,
                    DataType = ProviderEventType.BanchoPacket,
                    Data = new BanchoPacket(new FrameBundle {Frames = frameData})
                }));

        
        
        public async Task NotifySpectatorJoined(uint userId, uint spectatorUserId)
        {
            await _loggingManager.LogInfo<IStreamingProvider>("Spectator joined.", null, new
            {
                UserID = userId, 
                SpectatorUserID = spectatorUserId
            });
            
            using var streamerObservablesLock = await _streamerObservables.AcquireReadLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireReadLockGuard();
            using var spectatorCouplesLock = await _streamingPairs.AcquireLockGuard();

            spectatorCouplesLock.Value.Add(spectatorUserId, userId);
            
            await streamerObservablesLock.Value[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new HostSpectatorJoined {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.Streaming,
                DataType = ProviderEventType.BanchoPacket,
            });
            
            await spectatorObservablesLock.Value[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new FellowSpectatorJoined {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.Streaming,
                DataType = ProviderEventType.BanchoPacket,
            });
        }

        public async Task NotifySpectatorLeft(uint spectatorUserId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireReadLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireReadLockGuard();
            using var spectatorCouplesLock = await _streamingPairs.AcquireLockGuard();
   
            spectatorCouplesLock.Value.Remove(spectatorUserId, out var userId);
            
            await _loggingManager.LogInfo<IStreamingProvider>("Spectator left.", null, new
            {
                UserID = userId, 
                SpectatorUserID = spectatorUserId
            });
   
            await streamerObservablesLock.Value[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new HostSpectatorLeft {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.Streaming,
                DataType = ProviderEventType.BanchoPacket,
            });
            
            await spectatorObservablesLock.Value[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new FellowSpectatorLeft {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.Streaming,
                DataType = ProviderEventType.BanchoPacket,
            });
        }

        public async Task RegisterStreamer(uint userId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireWriteLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireWriteLockGuard();

            if (streamerObservablesLock.Value.ContainsKey(userId))
                return;

            streamerObservablesLock.Value.Add(userId, new InMemoryStreamerObservable());
            spectatorObservablesLock.Value.Add(userId, new InMemorySpectatorObservable());
        }

        public Task<bool> IsSpectating(uint spectatorUserId) =>
            _streamingPairs.LockAsync(pair => pair.ContainsKey(spectatorUserId));

        public async Task UnregisterStreamer(uint userId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireWriteLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireWriteLockGuard();
            
            if (streamerObservablesLock.Value.Remove(userId))
                await spectatorObservablesLock.Value[userId].Complete();
            
            spectatorObservablesLock.Value.Remove(userId);
        }
    }
}