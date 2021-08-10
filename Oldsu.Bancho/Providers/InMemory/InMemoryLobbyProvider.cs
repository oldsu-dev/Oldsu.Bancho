using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B904;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Multiplayer.Enums;
using Oldsu.Types;
using Oldsu.Utils.Threading;
using MatchDisband = Oldsu.Bancho.Packet.Shared.Out.MatchDisband;
using MatchUpdate = Oldsu.Bancho.Packet.Shared.Out.MatchUpdate;

namespace Oldsu.Bancho.Providers.InMemory
{
    // This need some future refactoring
    
    public class InMemoryMatchSetupObservable : InMemoryObservable<ProviderEvent>, IMatchSetupObservable { }
    public class InMemoryMatchGameObservable : InMemoryObservable<ProviderEvent>, IMatchGameObservable { }
    
    public class InMemoryLobbyProvider : InMemoryObservable<ProviderEvent>, ILobbyProvider
    {
        private InMemoryChannel _lobbyChatChannel;
        
        private const int MatchesAvailable = 256;

        private readonly AsyncRwLockWrapper<MatchState?[]> _matches;
        private readonly AsyncRwLockWrapper<InMemoryChannel?[]> _matchChatChannels;
        private readonly AsyncRwLockWrapper<InMemoryMatchSetupObservable?[]> _matchSetupObservables;
        private readonly AsyncRwLockWrapper<InMemoryMatchGameObservable?[]> _matchGameObservables;
        private readonly AsyncRwLockWrapper<Dictionary<uint, JoinedPlayerData>> _joinedPlayers;
        
        public InMemoryLobbyProvider()
        {
            _matches = new AsyncRwLockWrapper<MatchState?[]>(new MatchState?[MatchesAvailable]);
            _joinedPlayers = new AsyncRwLockWrapper<Dictionary<uint, JoinedPlayerData>>(new());
            
            _matchSetupObservables =
                new AsyncRwLockWrapper<InMemoryMatchSetupObservable?[]>(new InMemoryMatchSetupObservable[MatchesAvailable]);
            
            _matchGameObservables =
                new AsyncRwLockWrapper<InMemoryMatchGameObservable?[]>(new InMemoryMatchGameObservable[MatchesAvailable]);

            _matchChatChannels = new AsyncRwLockWrapper<InMemoryChannel?[]>(new InMemoryChannel[MatchesAvailable]);

            _lobbyChatChannel = new InMemoryChannel(new Channel
            {
                Tag = "#lobby",
                Topic = "Discussion about multiplayer matches.",
                AutoJoin = false,
                CanWrite = true,
                RequiredPrivileges = Privileges.Normal
            });
        }

        class JoinedPlayerData
        {
            public JoinedPlayerData(uint matchId)
            {
                MatchID = matchId;
                ScoreFrame = new AsyncMutexWrapper<ScoreFrame?>();
            }
            
            public uint MatchID { get; set; }
            
            public bool Playing { get; set; }
            
            public AsyncMutexWrapper<ScoreFrame?> ScoreFrame { get; set; }
            
            public bool CanSkip { get; set; }
            public bool CanLoad { get; set; }
        }
        
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
            
            await _matchSetupObservables.ReadAsync(async observables =>
                await (observables[data.MatchID]?.Notify(ev) ?? Task.CompletedTask));
            
            await _matchGameObservables.ReadAsync(async observables =>
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
            
            await _matchSetupObservables.ReadAsync(async observables =>
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
            
            await _matchSetupObservables.ReadAsync(async observables =>
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
            
            await _matchGameObservables.ReadAsync(async observables =>
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
            
            await _matchGameObservables.ReadAsync(async observables =>
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
            
            await _matchGameObservables.ReadAsync(async observables =>
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
            
            var evRequest = new ProviderEvent
            {
                ProviderType = ProviderType.Lobby,
                Data = UserRequestTypes.SubscribeToMatchSetup,
                DataType = ProviderEventType.UserRequest
            };            
            
            await _matchGameObservables.ReadAsync(async observables =>
            {
                var observable = observables[matchId];

                if (observable is not null)
                {
                    await observables[matchId]!.Notify(ev);
                    await observables[matchId]!.Notify(evRequest);
                }
            });
        }
        
        public async Task<MatchState?> GetMatchState(uint matchId)
        {
            using var matchesLock = await _matches.AcquireReadLockGuard();
            var match = matchesLock.Value[matchId];
            
            return (MatchState?)match?.Clone() ?? null;
        }

        public async Task<IMatchSetupObservable?> GetMatchSetupObservable(uint userId)
        {
            using var joinedPlayers = await _joinedPlayers.AcquireReadLockGuard();
            using var observablesLock = await _matchSetupObservables.AcquireReadLockGuard();

            if (!joinedPlayers.Value.TryGetValue(userId, out var playerData))
                return null;

            if (playerData.Playing)
                throw new UserPlayingException();

            return observablesLock.Value[playerData.MatchID];
        }
        
        public async Task<IMatchGameObservable?> GetMatchGameObservable(uint userId)
        {
            using var joinedPlayers = await _joinedPlayers.AcquireReadLockGuard();
            using var observablesLock = await _matchGameObservables.AcquireReadLockGuard();
            
            if (!joinedPlayers.Value.TryGetValue(userId, out var playerData))
                throw new UserNotInMatchException();

            if (!playerData.Playing)
                throw new UserNotPlayingException();

            return observablesLock.Value[playerData.MatchID];
        }


        public async Task<MatchState?> CreateMatch(uint userId, MatchSettings matchSettings)
        {
            MatchState match;
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (joinedPlayersLock.Value.ContainsKey(userId))
                    throw new UserAlreadyInMatchException();

                using var matchesLock = await _matches.AcquireWriteLockGuard();

                for (uint matchId = 0; matchId < MatchesAvailable; matchId++)
                {
                    if (matchesLock.Value[matchId] != null)
                        continue;

                    match = matchesLock.Value[matchId] = new MatchState((int) matchId, (int) userId, matchSettings);
                    _ = match.Join((int) userId, matchSettings.GamePassword!);

                    await _joinedPlayers.Upgrade(joinedPlayersLock);

                    joinedPlayersLock.Value.Add(userId, new JoinedPlayerData(matchId));

                    await _matchSetupObservables.WriteAsync(observables =>
                        observables[matchId] = new InMemoryMatchSetupObservable());
                    
                    await _matchChatChannels.WriteAsync(channels =>
                        channels[matchId] = new InMemoryChannel(new Channel
                        {
                            Tag = "#multiplayer",
                            Topic = "Multiplayer room discussion.",
                            AutoJoin = true,
                            CanWrite = true,
                            RequiredPrivileges = Privileges.Normal
                        }));
                    
                    match = (match.Clone() as MatchState)!;
                    
                    goto MatchCreated;
                }

                goto MatchNotCreated;
            }
            
            MatchCreated:
            await NotifyMatchUpdate(match);
            return match;
            
            MatchNotCreated:
            return null;
        }

        public Task<MatchState[]> GetAvailableMatches() =>
            _matches.ReadAsync(matches => matches.Where(m => m != null)
                .Select(m => (MatchState)m!.Clone()).ToArray())!;
        
        public async Task<MatchState?> JoinMatch(uint userId, uint matchId, string password)
        {
            MatchState match;
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (joinedPlayersLock.Value.ContainsKey(userId))
                    throw new UserAlreadyInMatchException();

                using var matchesLock = await _matches.AcquireReadLockGuard();
                
                var matchArrayLength = matchesLock.Value.Length;
                if (matchId >= matchArrayLength)
                    throw new InvalidMatchIDException();

                if (matchesLock.Value[matchId] == null)
                    throw new MatchNotFoundException();

                await _matches.Upgrade(matchesLock);

                match = matchesLock.Value[matchId]!;

                var newSlot = match!.Join((int) userId, password);
                if (newSlot is null)
                    return null;

                await _joinedPlayers.Upgrade(joinedPlayersLock);

                joinedPlayersLock.Value.Add(userId, new JoinedPlayerData(matchId));
            }

            await NotifyMatchUpdate(match);
            return match;
        }
        
        public async Task<bool> TryLeaveMatch(uint userId)
        {
            MatchState match;
            JoinedPlayerData playerData;

            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out playerData!))
                    return false;
                
                using (var matchesLock = await _matches.AcquireWriteLockGuard())
                {
                    match = matchesLock.Value[playerData.MatchID]!;
                    match.Leave(userId, out var disbandMatch);

                    await _joinedPlayers.Upgrade(joinedPlayersLock);
                    joinedPlayersLock.Value.Remove(userId);
                    
                    if (disbandMatch)
                    {
                        matchesLock.Value[playerData.MatchID] = null;
                        
                        await _matchChatChannels.WriteAsync(channels => channels[playerData.MatchID] = null);
                        await _matchSetupObservables.WriteAsync(observables => observables[playerData.MatchID] = null);
                        await _matchGameObservables.WriteAsync(observables => observables[playerData.MatchID] = null);
                            
                        goto MatchDisbanded;
                    }

                    match = (match.Clone() as MatchState)!;
                    goto PlayerLeft;
                }
            }

            MatchDisbanded:
            await NotifyMatchDisband(playerData.MatchID);
            return true;
            
            PlayerLeft:
            await NotifyMatchUpdate(match);
            return true;
        }
        
        private async Task ExecuteOperationOnCurrentMatch(uint userId, Func<MatchState, bool> fn)
        {
            MatchState match;
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (playerData.Playing)
                    throw new UserPlayingException();
                
                using (var matchesLock = await _matches.AcquireWriteLockGuard())
                {
                    match = matchesLock.Value[playerData.MatchID]!;
                    var notify = fn(match);

                    if (notify)
                    {
                        match = (match.Clone() as MatchState)!;
                        goto DoNotify;
                    }
                    
                    goto DontNotify;
                }
            }

            DoNotify:
            await NotifyMatchUpdate(match);
            
            DontNotify: ;
        }
        
        public Task MatchSetReady(uint userId) => 
            ExecuteOperationOnCurrentMatch(userId, 
                match =>
                {
                    match.Ready(userId);
                    return true;
                });

        public Task MatchGotBeatmap(uint userId) =>
            ExecuteOperationOnCurrentMatch(userId, 
                match =>
                {
                    match.GotBeatmap(userId);
                    return true;
                });

        public Task MatchSetUnready(uint userId) => 
            ExecuteOperationOnCurrentMatch(userId, 
                match =>
                {
                    match.Unready(userId);
                    return true;
                });
        
        public Task MatchMoveSlot(uint userId, uint newSlot) => 
            ExecuteOperationOnCurrentMatch(userId, 
                match => match.MoveSlot(userId, newSlot));
        
        public Task MatchChangeSettings(uint userId, MatchSettings matchSettings) => 
            ExecuteOperationOnCurrentMatch(userId, 
                match =>
                {
                    match.ChangeSettings(userId, matchSettings);
                    return true;
                });

        public Task MatchChangeTeam(uint userId)  =>
            ExecuteOperationOnCurrentMatch(userId, match =>
            {
                match.ChangeTeam(userId);
                return true;
            });
        
        public Task MatchNoBeatmap(uint userId) =>
            ExecuteOperationOnCurrentMatch(userId, match =>
            {
                match.NoBeatmap(userId);
                return true;
            });

        public Task MatchChangeMods(uint userId, short mods) =>
            ExecuteOperationOnCurrentMatch(userId, match =>
            {
                 match.ChangeMods(userId, mods);
                 return true;
            });

        public async Task<uint?> MatchLockSlot(uint userId, uint slot)
        {
            MatchState match;
            uint? kickUser;
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (playerData.Playing)
                    throw new UserPlayingException();
                
                using (var matchesLock = await _matches.AcquireWriteLockGuard())
                {
                    match = matchesLock.Value[playerData.MatchID]!;
                    
                    var notify = match.LockSlot(userId, slot, out kickUser);

                    if (kickUser is not null)
                    {
                        await _joinedPlayers.Upgrade(joinedPlayersLock);
                        joinedPlayersLock.Value.Remove(kickUser.Value);
                    }

                    if (notify)
                    {
                        match = (match.Clone() as MatchState)!;
                        goto DoNotify;
                    }   
                    
                    goto DontNotify;
                }
            }

            DoNotify:
            await NotifyMatchUpdate(match);
            
            DontNotify: ;
            return kickUser;
        }

        public async Task MatchStart(uint userId)
        {
            MatchState match;
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                uint[] playingUsers;
                
                using (var matchesLock = await _matches.AcquireWriteLockGuard())
                {
                    match = matchesLock.Value[playerData!.MatchID]!;
                    match.Start(userId);
                    
                    playingUsers = match.GetPlayingUsersIDs();
                }

                Array.ForEach(playingUsers, user =>
                {
                    playerData = joinedPlayersLock.Value[user];
                    
                    playerData.Playing = true;
                    playerData.ScoreFrame.SetValueAsync(new ScoreFrame());
                });
                
                await _matchGameObservables.WriteAsync(observables =>
                    observables[playerData.MatchID] = new InMemoryMatchGameObservable());
                
                match = (match.Clone() as MatchState)!;
            }

            await NotifyMatchUpdate(match);
            await NotifyMatchStart(match);
        }
        
        public async Task MatchScoreUpdate(uint userId, ScoreFrame scoreFrame)
        {
            uint matchId; 
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (!playerData.Playing)
                    throw new UserNotPlayingException();

                using var matchesLock = await _matches.AcquireReadLockGuard();
                
                scoreFrame.SlotID = (byte)matchesLock.Value[playerData.MatchID]!.GetSlotIndexByPlayerID(userId);

                await playerData.ScoreFrame.SetValueAsync(scoreFrame);
                
                matchId = playerData.MatchID;
            }

            await NotifyMatchScore(matchId, scoreFrame);
        }
        
        public async Task MatchSkip(uint userId)
        {
            uint matchId; 
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (!playerData.Playing)
                    throw new UserNotPlayingException();

                using var matchesLock = await _matches.AcquireWriteLockGuard();
                
                playerData.CanSkip = false;

                var match = matchesLock.Value[playerData.MatchID]!;
                match.Skip(userId);

                matchId = playerData.MatchID;
                
                if (match.AllSkipped)
                    goto DoNotifySkip;
                
                goto DontNotifySkip;
            }
            
            DoNotifySkip:
            await NotifyMatchSkip(matchId);
            
            DontNotifySkip: ;
        }
        
        public async Task MatchLoad(uint userId)
        {
            uint matchId; 
            
            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (!playerData.Playing)
                    throw new UserNotPlayingException();

                using var matchesLock = await _matches.AcquireWriteLockGuard();
                
                playerData.CanLoad = false;

                var match = matchesLock.Value[playerData.MatchID]!;
                match.Load(userId);

                matchId = playerData.MatchID;
                
                if (match.AllLoaded)
                    goto DoNotifyLoad;
                
                goto DontNotifyLoad;
            }
            
            DoNotifyLoad:
            await NotifyMatchLoad(matchId);
            
            DontNotifyLoad: ;
        }
        
        public async Task<bool> MatchComplete(uint userId)
        {
            uint matchId;
            MatchState match;

            using (var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard())
            {
                if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                    throw new UserNotInMatchException();

                if (!playerData.Playing)
                    throw new UserNotPlayingException();

                using var matchesLock = await _matches.AcquireWriteLockGuard();
                
                match = matchesLock.Value[playerData.MatchID]!;
                match.Complete(userId);

                matchId = playerData.MatchID;

                if (match.AllCompleted)
                {
                    Array.ForEach(match.GetPlayingUsersIDs(), user =>
                    {
                        playerData = joinedPlayersLock.Value[user];
                    
                        playerData.Playing = false;
                        playerData.ScoreFrame.SetValueAsync(null);
                    });
                        
                    match.Reset();
                    
                    match = (match.Clone() as MatchState)!;
                    
                    goto DoNotifyComplete;
                }

                goto DontNotifyComplete;
            }
            
            DoNotifyComplete:
            await NotifyMatchUpdate(match);
            await NotifyMatchComplete(matchId);
            return true;
            
            DontNotifyComplete: 
            return false;
        }

        public async Task SendMessageToMatch(uint userId, string username, string contents)
        {
            using var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard();
            
            if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                throw new UserNotInMatchException();

            await _matchChatChannels.ReadAsync(channels => 
                channels[playerData.MatchID]!.SendMessage(username, contents));
        }

        public Task SendMessageToLobby(string username, string contents) =>
            _lobbyChatChannel.SendMessage(username, contents);

        
        public async Task<bool> IsPlayerInMatch(uint userId)
        {
            using var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard();

            return joinedPlayersLock.Value.ContainsKey(userId);
        }

        public async Task<IChatChannel> GetMatchChatChannel(uint userId)
        {
            using var joinedPlayersLock = await _joinedPlayers.AcquireReadLockGuard();
            
            if (!joinedPlayersLock.Value.TryGetValue(userId, out var playerData))
                throw new UserNotInMatchException();
            
            return await _matchChatChannels.ReadAsync(channels => channels[playerData.MatchID]!);
        }

        public Task<IChatChannel> GetLobbyChatChannel() => Task.FromResult<IChatChannel>(_lobbyChatChannel);
        
    }
}