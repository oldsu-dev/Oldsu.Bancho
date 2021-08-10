using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryUnsubscriber<T> : IAsyncDisposable
    {
        private readonly AsyncRwLockWrapper<List<IAsyncObserver<T>>> _observers;
        private readonly IAsyncObserver<T> _observer;

        public InMemoryUnsubscriber(
            AsyncRwLockWrapper<List<IAsyncObserver<T>>> observers, 
            IAsyncObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        private volatile bool _disposing = false;

        public async ValueTask DisposeAsync()
        {
            if (_disposing)
                return;

            _disposing = true;

            await _observers.WriteAsync(observers =>
            {
                observers.Remove(_observer);
            });
        }
    }
}