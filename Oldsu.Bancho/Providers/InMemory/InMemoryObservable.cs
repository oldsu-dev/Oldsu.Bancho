using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryObservable<T> : IAsyncObservable<T>
    {
        private readonly AsyncRwLockWrapper<List<IAsyncObserver<T>>> _observers;

        protected InMemoryObservable()
        {
            _observers = new AsyncRwLockWrapper<List<IAsyncObserver<T>>>(new());
        }

        public Task<IAsyncDisposable> Subscribe(IAsyncObserver<T> observer) =>
            _observers.WriteAsync(observers =>
            {
                observers.Add(observer);
                return (IAsyncDisposable)new InMemoryUnsubscriber<T>(_observers, observer);
            });

        public Task Notify(T data) =>
            _observers.ReadAsync(observers => 
                observers.ForEach(observer => observer.OnNext(this, data)));

        public Task Complete() => 
            _observers.ReadAsync(observers => 
                observers.ForEach(observer => observer.OnCompleted(this)));

        public Task Error(Exception exception) => 
            _observers.ReadAsync(observers => 
                observers.ForEach(observer => observer.OnError(this, exception)));
    }
}