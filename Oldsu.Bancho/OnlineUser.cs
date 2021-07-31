using System;
using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Spectating;
using Oldsu.Types;
using Oldsu.Utils.Threading;
using Action = Oldsu.Enums.Action;

namespace Oldsu.Bancho
{
    public class OnlineUser
    {
        public Server.Mediator ServerMediator { get; }
        
        public Match.Mediator? MatchMediator { get; set; }
        
        public AuthenticatedConnection Connection { get; }
        
        public User UserInfo { get; }
        public Presence Presence { get; }

        public AsyncRwLockWrapper<Activity> Activity { get; }
        public AsyncRwLockWrapper<Stats?> Stats { get; }

        private readonly AsyncRwLockWrapper<GameSpectator> _gameSpectator;
        private readonly AsyncRwLockWrapper<GameBroadcaster> _gameBroadcaster;

        public event EventHandler? Left;
        
        public OnlineUser(Server.Mediator serverMediator, AuthenticatedConnection connection, 
            User userInfo, Presence presence, Stats? stats)
        {
            Connection = connection;
            UserInfo = userInfo;
            Presence = presence;

            Stats = new AsyncRwLockWrapper<Stats?>(stats);
            Activity = new AsyncRwLockWrapper<Activity>(new Activity {Action = Action.Idle});
            
            ServerMediator = serverMediator;

            _gameBroadcaster = new AsyncRwLockWrapper<GameBroadcaster>(new GameBroadcaster());
            _gameSpectator = new AsyncRwLockWrapper<GameSpectator>(new GameSpectator());
            
            Connection.PacketReceived += (_, packet) => packet.Handle(this);
            Connection.Disconnected += HandleDisconnection;
        }

        public async void HandleDisconnection(object? sender, EventArgs _)
        {
            await StopSpectatingAsync();
            
            Left?.Invoke(this, EventArgs.Empty);
        }
        
        public async Task<bool> StartSpectatingAsync(OnlineUser targetUser)
        {
            using var spectatorLock = await _gameSpectator.AcquireWriteLockGuard();
            using var broadcasterLock = await targetUser._gameBroadcaster.AcquireWriteLockGuard();
            
            if ((~spectatorLock).SpectatingUser != null)
                return false;

            if ((~broadcasterLock).Spectators.ContainsKey(this.UserInfo.UserID))
                return false;

            (~spectatorLock).SpectatingUser = targetUser;
            (~broadcasterLock).Spectators.Add(this.UserInfo.UserID, this);

            return true;
        }

        public async Task<OnlineUser?> StopSpectatingAsync()
        {
            using var spectatorLock = await _gameSpectator.AcquireWriteLockGuard();

            var targetUser = (~spectatorLock).SpectatingUser;
            if (targetUser == null)
                return null;

            using var broadcasterLock = await targetUser._gameBroadcaster.AcquireWriteLockGuard();
            
            if (!(~broadcasterLock).Spectators.ContainsKey(this.UserInfo.UserID))
                return null;
            
            (~spectatorLock).SpectatingUser = null;
            (~broadcasterLock).Spectators.Remove(this.UserInfo.UserID);
            
            return targetUser;
        }
        
        public async Task BroadcastPacketToSpectators(BanchoPacket packet)
        {
            await _gameBroadcaster.ReadAsync(broadcaster =>
            {
                foreach (var spectator in broadcaster.Spectators.Values)
                    spectator.Connection.SendPacket(packet);
            });
        }
        
        public async Task HandshakeAsync()
        {
            await Connection.SendPacketAsync(new BanchoPacket(
                new Login() { LoginStatus = (int)UserInfo.UserID }));

            await Connection.SendPacketAsync(new BanchoPacket(
                new BanchoPrivileges { Privileges = Presence.Privilege }));
            
            await ServerMediator.Users.ReadAsync(async clients =>
            {
                using (var statsLock = await Stats.AcquireReadLockGuard())
                using (var activityLock = await Activity.AcquireReadLockGuard())
                {
                    clients.BroadcastPacketToOthers(new BanchoPacket(
                            new SetPresence
                            {
                                Stats = ~statsLock, Activity = ~activityLock, 
                                User = UserInfo, Presence = Presence
                            }), 
                        this.UserInfo.UserID);
                }

                foreach (var c in clients.Values)
                {
                    using var statsLock = await c.Stats.AcquireReadLockGuard();
                    using var activityLock = await c.Activity.AcquireReadLockGuard();
                    
                    await Connection.SendPacketAsync(new BanchoPacket(
                        new SetPresence
                        {
                            Activity = ~activityLock, Stats = ~statsLock,
                            User = c.UserInfo, Presence = c.Presence
                        }));
                }
            });

            await Connection.SendPacketAsync(new BanchoPacket( 
                new JoinChannel { ChannelName = "#osu" }));
        }

    }
}