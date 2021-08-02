using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryUserDataProvider : IUserDataProvider
    {
        private readonly AsyncRwLockWrapper<Dictionary<uint, UserData>> _wrapper;
        private readonly AsyncRwLockWrapper<List<IAsyncObserver<BanchoPacket>>> _observers;

        public InMemoryUserDataProvider()
        {
            _wrapper = new AsyncRwLockWrapper<Dictionary<uint, UserData>>();
            _observers = new AsyncRwLockWrapper<List<IAsyncObserver<BanchoPacket>>>();
        }
        
        public async Task<IAsyncDisposable> SubscribeAsync(IAsyncObserver<BanchoPacket> observer)
        {
            return await _observers.WriteAsync(observers =>
            {
                observers.Add(observer);
                return new BanchoPacketUnsubscriber(_observers, observer);;
            });
        }

        private class BanchoPacketUnsubscriber : IAsyncDisposable
        {
            private readonly AsyncRwLockWrapper<List<IAsyncObserver<BanchoPacket>>> _observers;
            private readonly IAsyncObserver<BanchoPacket> _observer;

            public BanchoPacketUnsubscriber(
                AsyncRwLockWrapper<List<IAsyncObserver<BanchoPacket>>> observers, 
                IAsyncObserver<BanchoPacket> observer)
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
                    if (observers.Contains(_observer)) 
                        observers.Remove(_observer);
                });
            }
        }

        private Task NotifyObservers(BanchoPacket packet) =>
            _observers.ReadAsync(observers =>
                observers.ForEach(observer => observer.OnNextAsync(packet)));

        public async Task RegisterUserAsync(uint userId, UserData data)
        {
            var newData = await _wrapper.WriteAsync(users =>
            {
                users.Add(userId, data);
                return (UserData)data.Clone();
            });

            await NotifyObservers(new BanchoPacket(SetPresence.FromUserData(newData)));
        }

        public async Task UnregisterUserAsync(uint userId)
        {
            await _wrapper.WriteAsync(users => users.Remove(userId));
            
            await NotifyObservers(new BanchoPacket(new UserQuit {UserID = (int)userId}));
        }

        public Task<IEnumerable<UserData>> GetAllUsersAsync() =>
            _wrapper.ReadAsync(users => (IEnumerable<UserData>)users.Values.ToArray());

        public async Task SetActivityAsync(uint userId, Activity activity)
        {
            var newData = await _wrapper.WriteAsync(users =>
            {
                users[userId].Activity = activity;
                return (UserData)users[userId].Clone();
            });

            await NotifyObservers(new BanchoPacket(
                StatusUpdate.FromUserData(newData, Completeness.Self)));
        }

        public async Task SetStatsAsync(uint userId, StatsWithRank? stats)
        {
            var newData = await _wrapper.WriteAsync(users =>
            {
                users[userId].Stats = stats;
                return (UserData)users[userId].Clone();
            });

            await NotifyObservers(new BanchoPacket(
                StatusUpdate.FromUserData(newData, Completeness.Self)));
        }

        public Task<UserData> GetUserAsync(uint userId) =>
            _wrapper.ReadAsync(users => (UserData) users[userId].Clone());
        
        public Task<Privileges> GetUserPrivilegesAsync(uint userId) =>
            _wrapper.ReadAsync(users => users[userId].Presence.Privilege);
    }
}