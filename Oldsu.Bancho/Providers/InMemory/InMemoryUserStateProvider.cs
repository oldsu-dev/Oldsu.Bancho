using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Types;
using Oldsu.Utils;
using Oldsu.Utils.Threading;
using Action = System.Action;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryUserStateProvider : InMemoryObservable<ProviderEvent>, IUserStateProvider
    {
        private readonly AsyncRwLockWrapper<Dictionary<uint, UserData>> _wrapper;
        
        private readonly AsyncMutexWrapper<LinkedList<(uint Rank, uint UserID)>[]> _ranksCache;

        private readonly LoggingManager _loggingManager;
        
        public InMemoryUserStateProvider(LoggingManager loggingManager)
        {
            _loggingManager = loggingManager;
            _wrapper = new AsyncRwLockWrapper<Dictionary<uint, UserData>>(new ());

            _ranksCache = new AsyncMutexWrapper<LinkedList<(uint, uint)>[]>(new LinkedList<(uint, uint)>[4]);
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
            var unregistered = await _wrapper.WriteAsync(users => users.Remove(userId));

            if (unregistered)
            {
                await Notify(new ProviderEvent
                {
                    ProviderType = ProviderType.UserState,
                    DataType = ProviderEventType.BanchoPacket,
                    Data = new BanchoPacket(new UserQuit {UserID = (int) userId})
                });
            }
        }

        public Task<IEnumerable<UserData>> GetAllUsersAsync() =>
            _wrapper.ReadAsync(users => (IEnumerable<UserData>)users.Values.ToArray());

        public async Task SetActivityAsync(uint userId, Activity activity)
        {
            await _loggingManager.LogInfo<IUserStateProvider>(
                "User updated activity",
                null,
                new
                {
                    UserID = userId,
                    Activity = activity
                });
            
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

        public async Task SetStatsAsync(uint userId, Mode gamemode,  StatsWithRank? stats)
        {
            await _loggingManager.LogInfo<IUserStateProvider>(
                "User updated stats",
                null,
                new
                {
                    UserID = userId,
                    Stats = stats
                });

            using var wrapperLock = await _wrapper.AcquireWriteLockGuard();
            
            var previousUserData = wrapperLock.Value[userId];
            wrapperLock.Value[userId].Stats = stats;

            var newData = (wrapperLock.Value[userId].Clone() as UserData)!; 
            
            // Rank caching
            bool rankNotChanged = stats != null && previousUserData.Stats != null
                                  && previousUserData.Stats.Mode == gamemode
                                  && previousUserData.Stats.Rank == stats.Rank;
            
            List<UserData> shiftedRankUserData = new List<UserData>();
            
            if (stats != null && !rankNotChanged)
            {
                using var rankCacheLock = await _ranksCache.AcquireLockGuard();

                var rank = stats.Rank;
                var modeLinkedList = rankCacheLock.Value[(uint) gamemode];
                
                if (previousUserData.Stats != null)
                    rankCacheLock.Value[(uint) previousUserData.Stats!.Mode].Remove((previousUserData.Stats.Rank, userId));
                
                var node = modeLinkedList.First;


                for (; node != null; node = node.Next)
                {
                    if (rank < node.ValueRef.Rank)
                    {
                        modeLinkedList.AddBefore(node, (rank, userId));
                        break;
                    }

                    // if rank is equal to a node, all subsequent nodes must be shifted 
                    if (rank == node.ValueRef.Rank)
                    {
                        modeLinkedList.AddBefore(node, (rank, userId));

                        for (; node != null; node = node.Next)
                        {
                            node.ValueRef.Rank += 1;
                            
                            var userData = wrapperLock.Value[node.ValueRef.UserID];
                            shiftedRankUserData.Add((userData.Clone() as UserData)!);
                        }

                        break;
                    }
                    
                    // no greater rankings found, add it at the end
                    if (node.Next == null)
                        modeLinkedList.AddAfter(node, (rank, userId));
                }
            }

            foreach (var data in shiftedRankUserData)
                await Notify(new ProviderEvent {
                    ProviderType = ProviderType.UserState,
                    DataType = ProviderEventType.BanchoPacket,
                    Data = new BanchoPacket(StatusUpdate.FromUserData(data, Completeness.Self))
                });

            await Notify(new ProviderEvent {
                ProviderType = ProviderType.UserState,
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(StatusUpdate.FromUserData(newData, Completeness.Self))
            });
        }

        
        public Task<UserData?> GetUser(uint userId) =>
            _wrapper.ReadAsync(users => (UserData?)users!.GetValueOrDefault(userId, null)?.Clone());

        public Task<bool> IsUserOnline(uint userId) =>
            _wrapper.ReadAsync(users => users.ContainsKey(userId));

        public Task<UserData> GetUserAsync(uint userId) =>
            _wrapper.ReadAsync(users => (UserData) users[userId].Clone());
        
        public Task<Privileges> GetUserPrivilegesAsync(uint userId) =>
            _wrapper.ReadAsync(users => users[userId].Presence.Privilege);
    }
}