using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Oldsu.Bancho.Packet;
using Oldsu.Utils;

namespace Oldsu.Bancho.Multiplayer
{
    public class Lobby
    {
        public const int MatchesAvailable = 256;
        
        private readonly Dictionary<uint, Client> _clientsInLobby = new();
        private readonly ReaderWriterLockSlim _rwLock = new();

        private Match?[] _matches = new Match?[MatchesAvailable];

        public RwLockableEnumerable<Match?> Matches => new(_rwLock, _matches);

        public bool RegisterMatch(Match match)
        {
            _rwLock.EnterWriteLock();

            try
            {
                for (byte i = 0; i <= (MatchesAvailable - 1); i++)
                    if (_matches[i] == null)
                    {
                        _matches[i] = match;
                        match.MatchID = i;
                        return true;
                    }

                return false;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool DisbandMatch(int id, int clientId)
        {
            _rwLock.EnterWriteLock();

            try
            {
                if (_matches[id] == null || clientId != _matches[id]!.HostID) 
                    return false;
                
                _matches[id] = null;
                return true;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        
        public void AddPlayer(Client client)
        {
            _rwLock.EnterWriteLock();

            try
            {
                _clientsInLobby.TryAdd(client.ClientContext!.User.UserID, client);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void RemovePlayer(Client client)
        {
            _rwLock.EnterWriteLock();

            try
            {
                _clientsInLobby.Remove(client.ClientContext!.User.UserID, out _);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void UpdateLobby(BanchoPacket packet)
        {
            _rwLock.EnterReadLock();

            try
            {
                foreach (var client in _clientsInLobby.Values)
                {
                    client.SendPacket(packet);
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            
        }
    }
}