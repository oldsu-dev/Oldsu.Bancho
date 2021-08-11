using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryUserStateProvider : InMemoryObservable<ProviderEvent>, IUserStateProvider
    {
        private readonly AsyncRwLockWrapper<Dictionary<uint, UserData>> _wrapper;

        public InMemoryUserStateProvider()
        {
            _wrapper = new AsyncRwLockWrapper<Dictionary<uint, UserData>>(new ());
        }

        public async Task RegisterUserAsync(uint userId, UserData data)
        {
            await _wrapper.WriteAsync(async users =>
            {
                await Notify(new ProviderEvent {
                    ProviderType = ProviderType.UserState,
                    DataType = ProviderEventType.BanchoPacket,
                    Data = new BanchoPacket(SetPresence.FromUserData((UserData)data.Clone()))
                });
                
                if (users.ContainsKey(userId))
                    return;
                
                users.Add(userId, data);
            });
        }

        public async Task UnregisterUserAsync(uint userId)
        {
            await _wrapper.WriteAsync(users => users.Remove(userId));
            
            await Notify(new ProviderEvent {
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(new UserQuit {UserID = (int)userId})
            });
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

            await Notify(new ProviderEvent {
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(StatusUpdate.FromUserData(newData, Completeness.Self))
            });
        }

        public async Task SetStatsAsync(uint userId, StatsWithRank? stats)
        {
            var newData = await _wrapper.WriteAsync(users =>
            {
                users[userId].Stats = stats;
                return (UserData)users[userId].Clone();
            });

            await Notify(new ProviderEvent {
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(StatusUpdate.FromUserData(newData, Completeness.Self))
            });
        }

        public Task<UserData> GetUserAsync(uint userId) =>
            _wrapper.ReadAsync(users => (UserData) users[userId].Clone());
        
        public Task<Privileges> GetUserPrivilegesAsync(uint userId) =>
            _wrapper.ReadAsync(users => users[userId].Presence.Privilege);
    }
}