using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Exceptions.GameBroadcaster;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Logging;
using Oldsu.Types;

namespace Oldsu.Bancho.GameLogic
{
    public class UserPanelManagerEntity
    {
        private Dictionary<uint, UserPanelManagerEntity> _spectators;

        public User User { get; }
        
        public UserPanelManagerEntity? SpectatingEntity { get; set; }
        public IEnumerable<UserPanelManagerEntity> Spectators => _spectators.Values;

        public bool MutedInChat { get; set; } = false;
        
        public UserPanelManagerEntity(User user)
        {
            User = user;
            SpectatingEntity = null;
            _spectators = new Dictionary<uint, UserPanelManagerEntity>();
        }

        public void AddSpectator(UserPanelManagerEntity entity)
        {
            if (!_spectators.TryAdd(entity.User.UserID, entity))
                throw new AlreadySpectatingException();
            
            entity.SpectatingEntity = this;
            
            User.SendPacket(new HostSpectatorJoined {UserID = (int)entity.User.UserID});
            BroadcastToSpectators(new FellowSpectatorJoined {UserID = (int)entity.User.UserID});
        }

        public void RemoveSpectator(UserPanelManagerEntity entity)
        {
            if (!_spectators.Remove(entity.User.UserID))
                throw new NotSpectatingException();

            entity.SpectatingEntity = null;
            
            User.SendPacket(new HostSpectatorLeft() {UserID = (int)entity.User.UserID});
            BroadcastToSpectators(new FellowSpectatorLeft {UserID = (int)entity.User.UserID});
        }
        
        public void BroadcastToSpectators(SharedPacketOut packetOut)
        {
            CachedBanchoPacket packet = new CachedBanchoPacket(packetOut);
            
            foreach (var entity in Spectators) 
                entity.User.SendPacket(packet);
        }
    }
    
    public class UserPanelManager
    {
        private readonly Dictionary<uint, UserPanelManagerEntity> _entitiesByUserID;
        private readonly Dictionary<string, UserPanelManagerEntity> _entitiesByUsername;

        public IReadOnlyDictionary<uint, UserPanelManagerEntity> EntitiesByUserID => _entitiesByUserID;
        public IReadOnlyDictionary<string, UserPanelManagerEntity> EntitiesByUsername => _entitiesByUsername;
        
        public IEnumerable<UserPanelManagerEntity> Entities => _entitiesByUserID.Values;

        private readonly LoggingManager _loggingManager;
        
        public UserPanelManager(LoggingManager loggingManager)
        {
            _entitiesByUsername = new Dictionary<string, UserPanelManagerEntity>();
            _entitiesByUserID = new Dictionary<uint, UserPanelManagerEntity>();
            
            _loggingManager = loggingManager;
        }

        private void BroadcastStatusUpdate(StatusUpdate statusUpdate) => 
            BroadcastPacket(statusUpdate);
        
        public void BroadcastPacket(SharedPacketOut packetOut)
        {
            CachedBanchoPacket packet = new CachedBanchoPacket(packetOut);
            
            foreach (var entity in Entities) 
                entity.User.SendPacket(packet);
        }
        
        public void RegisterUser(User user)
        {
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "User registered", dump: new
                {
                    user.UserID
                });
                
            #endregion

            foreach (var otherEntity in Entities)
                user.SendPacket(SetPresence.FromUserData(otherEntity.User));
            
            UserPanelManagerEntity entity = new UserPanelManagerEntity(user);
            
            _entitiesByUserID.Add(user.UserID, entity); 
            _entitiesByUsername.Add(user.Username, entity);
            
            BroadcastPacket(SetPresence.FromUserData(user));
        }

        public void UnregisterUser(User user)
        {
            _entitiesByUserID.Remove(user.UserID);
            _entitiesByUsername.Remove(user.Username);
            
            #region Logging

            _loggingManager.LogInfoSync<UserPanelManager>("User unregistered", dump: new
            {
                user.UserID
            });

            #endregion
            
            BroadcastPacket(new UserQuit {UserID = (int)user.UserID});
        }

        public void StartSpectating(User user, uint targetUserId)
        {
            UserPanelManagerEntity selfEntity = _entitiesByUserID[user.UserID];

            if (selfEntity.SpectatingEntity != null)
                StopSpectating(user);
            
            UserPanelManagerEntity targetEntity = _entitiesByUserID[targetUserId];
            
            targetEntity.AddSpectator(selfEntity);
            
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "Started spectating", dump: new
                {
                    user.UserID,
                    TargetUserID = targetUserId
                });
                
            #endregion
        }
 
        public void StopSpectating(User user)
        {
            UserPanelManagerEntity selfEntity = _entitiesByUserID[user.UserID];

            if (selfEntity.SpectatingEntity == null)
                throw new NotSpectatingException();

            selfEntity.SpectatingEntity.RemoveSpectator(selfEntity);
            
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "Stopped spectating", dump: new
                {
                    user.UserID
                });
                
            #endregion
        }
        
        public bool IsOnline(uint userId) =>
            _entitiesByUserID.ContainsKey(userId);

        public void UpdateActivity(User user, Activity activity)
        {
            user.Activity = activity;
         
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "User updated activity", dump: new
                {
                    user.UserID,
                    user.Activity
                });
                
            #endregion
            
            BroadcastStatusUpdate(StatusUpdate.FromUserData(user, Completeness.Online));
        }
        
        public void UpdateRank(uint userId, uint rank)
        {
            User user = EntitiesByUserID[userId].User;
            user.Stats!.Rank = rank;
            
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "Rank updated", dump: new
                {
                    userId,
                    rank
                });

            #endregion
            
            BroadcastStatusUpdate(StatusUpdate.FromUserData(user, Completeness.Online));
        }
        
        public void UpdateStats(User user, StatsWithRank stats)
        {
            user.Stats = stats;
            
            #region Logging
            
            _loggingManager.LogInfoSync<UserPanelManager>(
                "User updated stats", dump: new
                {
                    user.UserID,
                    user.Stats
                });

            #endregion
            
            BroadcastStatusUpdate(StatusUpdate.FromUserData(user, Completeness.Online));
        }
    }
}