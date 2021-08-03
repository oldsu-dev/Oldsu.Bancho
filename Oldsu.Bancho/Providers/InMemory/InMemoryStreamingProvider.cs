using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;
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

        public InMemoryStreamingProvider()
        {
            _streamerObservables = new AsyncRwLockWrapper<Dictionary<uint, InMemoryStreamerObservable>>();
            _spectatorObservables = new AsyncRwLockWrapper<Dictionary<uint, InMemorySpectatorObservable>>();
            _streamingPairs = new AsyncMutexWrapper<Dictionary<uint, uint>>();
        }

        public Task<IStreamerObservable> GetStreamerObserver(uint userId) => 
            _streamerObservables.ReadAsync(
                observables => (IStreamerObservable)observables[userId]);

        public Task<ISpectatorObservable> GetSpectatorObserver(uint userId) =>
            _spectatorObservables.ReadAsync(
                observables => (ISpectatorObservable)observables[userId]);

        public Task PushFrames(uint userId, byte[] frameData) =>
            _spectatorObservables.ReadAsync(observables => 
                observables[userId].Notify(new ProviderEvent
                {
                    ProviderType = ProviderType.UserState,
                    DataType = ProviderEventType.BanchoPacket,
                    Data = new BanchoPacket(new FrameBundle {Frames = frameData})
                }));

        public async Task NotifySpectatorJoined(uint userId, uint spectatorUserId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireReadLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireReadLockGuard();
            using var spectatorCouplesLock = await _streamingPairs.AcquireLockGuard();
   
            (~spectatorCouplesLock).Add(spectatorUserId, userId);
            
            await (~streamerObservablesLock)[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new HostSpectatorJoined {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
            });
            
            await (~spectatorObservablesLock)[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new FellowSpectatorJoined {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
            });
            
        }

        public async Task NotifySpectatorLeft(uint spectatorUserId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireReadLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireReadLockGuard();
            using var spectatorCouplesLock = await _streamingPairs.AcquireLockGuard();
   
            (~spectatorCouplesLock).Remove(spectatorUserId, out var userId);
   
            await (~streamerObservablesLock)[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new HostSpectatorLeft {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
            });
            
            await (~spectatorObservablesLock)[userId].Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new FellowSpectatorLeft {UserID = (int)spectatorUserId}),
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
            });
        }

        public async Task RegisterStreamer(uint userId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireWriteLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireWriteLockGuard();
            
            (~streamerObservablesLock).Add(userId, new InMemoryStreamerObservable());
            (~spectatorObservablesLock).Add(userId, new InMemorySpectatorObservable());
        }

        public async Task UnregisterStreamer(uint userId)
        {
            using var streamerObservablesLock = await _streamerObservables.AcquireWriteLockGuard();
            using var spectatorObservablesLock = await _spectatorObservables.AcquireWriteLockGuard();
            
            (~streamerObservablesLock).Remove(userId);
            (~spectatorObservablesLock)[userId].Complete();
            (~spectatorObservablesLock).Remove(userId);
        }
    }
}