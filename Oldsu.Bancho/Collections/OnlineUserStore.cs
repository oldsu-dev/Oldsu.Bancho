using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Utils;

namespace Oldsu.Bancho.Collections
{
    public class OnlineUserStore
    {
        private readonly MultiKeyDictionary<string, uint, OnlineUser> _clients;

        public OnlineUserStore()
        {
            _clients = new MultiKeyDictionary<string, uint, OnlineUser>();
        }

        public IEnumerable<OnlineUser> Values => _clients.Values;

        public bool TryGetValue(string key1, out OnlineUser value)
        {
            return _clients.TryGetValue(key1, out value);
        }

        public bool TryGetValue(uint key2, out OnlineUser value)
        {
            return _clients.TryGetValue(key2, out value);
        }

        public bool TryAdd(string key1, uint key2, OnlineUser value)
        {
            return _clients.TryAdd(key1, key2, value);
        }

        public bool TryRemove(string key1, uint key2, out OnlineUser value)
        {
            return _clients.TryRemove(key1, key2, out value);
        }

        /// <summary>
        ///     Broadcasting to all clients.
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        public void BroadcastPacket(BanchoPacket packet)
        {
            foreach (var c in _clients.Values)
                c.Connection.SendPacket(packet);
        }

        /// <summary>
        ///     Broadcasting to other clients. 
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        /// <param name="id">Id of client to avoid</param>
        public void BroadcastPacketToOthers(BanchoPacket packet, uint id)
        {
            foreach (var c in _clients.Values.Where(c => c.UserInfo.UserID != id))
                c.Connection.SendPacket(packet);
        }
        
        /// <summary>
        ///     Send packet to a specific user in the server.
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="username">Username of the user to send the packet to.</param>
        public bool SendPacketToSpecificUser(BanchoPacket packet, string username)
        {
            var hasClient = _clients.TryGetValue(username, out var user);
            
            if (hasClient)
                user.Connection.SendPacket(packet);

            return hasClient;
        }
    }
}