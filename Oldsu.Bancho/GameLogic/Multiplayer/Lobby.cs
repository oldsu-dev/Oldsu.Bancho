using System.Collections.Generic;
using System.Linq;
using DnsClient.Internal;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Logging;

namespace Oldsu.Bancho.GameLogic.Multiplayer
{
    public class Lobby
    {
        private const int MaxMatches = 256;
        
        private Match?[] _matches { get; }

        private IEnumerable<Match> AvailableMatches
        {
            get
            {
                for (int i = 0; i < _matches.Length; i++)
                {
                    if (_matches[i] != null)
                        yield return _matches[i]!;
                }
            }
        }

        public void SendMessage(User sender, string contents) =>
            _chatChannel.SendMessage(sender, contents);
        
        public Match GetMatchByID(uint matchId)
        {
            if (matchId >= MaxMatches || _matches[matchId] == null)
                throw new InvalidMatchIDException();

            return _matches[matchId]!;
        }

        private readonly ChatChannel _chatChannel;

        private readonly LoggingManager _loggingManager;
        
        public Lobby(LoggingManager loggingManager)
        {
            _loggingManager = loggingManager;
            _matches = new Match[MaxMatches];
            _chatChannel = new ChatChannel("#lobby", loggingManager);
        }

        private void BroadcastToLobby(SharedPacketOut packet)
        {
            CachedBanchoPacket cachedBanchoPacket =
                new CachedBanchoPacket(packet);
                        
            foreach (var user in _chatChannel.Users)
                user.SendPacket(cachedBanchoPacket);
        }

        public void Join(User user)
        {
            _chatChannel.Join(user);
            
            foreach (var match in AvailableMatches)
                user.SendPacket(new MatchUpdate{Match = match});
        }

        public void Leave(User user)
        {
            _chatChannel.Leave(user);
        }
        
        public void TryCreateMatch(User host, MatchSettings settings)
        {
            for (int i = 0; i < _matches.Length; i++)
            {
                if (_matches[i] == null)
                { 
                    Match match = new Match(i, host, settings, _loggingManager);

                    match.OnDisband += match =>
                    {
                        _matches[match.MatchID] = null;
                        BroadcastToLobby(new MatchDisband{MatchID = match.MatchID});
                    };

                    match.OnUpdate += match =>
                    {
                        BroadcastToLobby(new MatchUpdate{Match = match});
                    };

                    host.Match = match;
                    _matches[i] = match;
                    
                    BroadcastToLobby(new MatchUpdate{Match = match});

                    #region Logging

                    _loggingManager.LogInfoSync<Lobby>("Created a match", dump: new
                    {
                        host.UserID,
                        host.Match.Settings
                    });

                    #endregion
                    
                    return;
                }
            }
            
            host.SendPacket(new MatchJoinFail());
        }
    }
}