using System.Collections.Generic;
using System.Threading;
using Oldsu.Bancho.Packet;

namespace Oldsu.Bancho.Multiplayer
{
    public class Lobby
    {
        private readonly Dictionary<uint, Client> _clientsInLobby = new();
        private ReaderWriterLockSlim _rwLock = new();

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