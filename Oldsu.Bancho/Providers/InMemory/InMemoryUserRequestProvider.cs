using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Exceptions.UserRequest;
using Oldsu.Logging;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryUserRequestObservable : InMemoryObservable<ProviderEvent>, IUserRequestObservable
    { }

    public class InMemoryUserRequestProvider : IUserRequestProvider
    {
        private readonly LoggingManager _loggingManager;
        
        public InMemoryUserRequestProvider(LoggingManager loggingManager)
        {
            _loggingManager = loggingManager;
            _observables = new AsyncRwLockWrapper<Dictionary<uint, InMemoryUserRequestObservable>>(new());
        }
        
        private AsyncRwLockWrapper<Dictionary<uint, InMemoryUserRequestObservable>> _observables;

        public async Task RegisterUser(uint userId)
        {
            using var observablesLock = await _observables.AcquireWriteLockGuard();
            observablesLock.Value.Add(userId, new InMemoryUserRequestObservable());
        }

        public async Task UnregisterUser(uint userId)
        {
            using var observablesLock = await _observables.AcquireWriteLockGuard();
            observablesLock.Value.Remove(userId);
        }

        public async Task QuitMatch(uint userId)
        {
            using var observableLock = await _observables.AcquireReadLockGuard();

            if (!observableLock.Value.TryGetValue(userId, out var observable))
                throw new UserNotFoundException();

            await _loggingManager.LogInfo<ILobbyProvider>(
                "Send quit user request.", 
                null, 
                new
                {
                    UserID = userId
                });
            
            await observable.Notify(new ProviderEvent
            {
                Data = UserRequestTypes.QuitMatch,
                DataType = ProviderEventType.UserRequest,
                ProviderType = ProviderType.UserRequest
            });
        }

        public async Task AnnounceTransferHost(uint userId)
        {
            using var observableLock = await _observables.AcquireReadLockGuard();

            if (!observableLock.Value.TryGetValue(userId, out var observable))
                throw new UserNotFoundException();

            await _loggingManager.LogInfo<ILobbyProvider>(
                "Send announce transfer host request.", 
                null, 
                new
                {
                    UserID = userId
                });
            
            await observable.Notify(new ProviderEvent
            {
                Data = UserRequestTypes.AnnounceTransferHost,
                DataType = ProviderEventType.UserRequest,
                ProviderType = ProviderType.UserRequest
            });
        }

        public async Task<IUserRequestObservable> GetObservable(uint userId)
        {
            using var observableLock = await _observables.AcquireReadLockGuard();

            if (!observableLock.Value.TryGetValue(userId, out var observable))
                throw new UserNotFoundException();

            return observable;
        }
    }
}