using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Multiplayer
{
    /*public class Lobby
    {
        public const int MatchesAvailable = 256;
        
        private readonly Dictionary<uint, OnlineUser> _clientsInLobby = new();
        private readonly AsyncRwLockWrapper<Match>?[] _matches = new AsyncRwLockWrapper<Match>?[MatchesAvailable];

        public AsyncRwLockWrapper<Match>? RegisterMatch(Match match)
        {
            for (byte i = 0; i <= (MatchesAvailable - 1); i++)
                if (_matches[i] == null)
                {
                    _matches[i] = new AsyncRwLockWrapper<Match>();
                    match.MatchID = i;
                    return _matches[i]!;
                }

            return null;
        }

        public void DisbandMatch(int id)
        {
            if (_matches[id] != null)
                return false;
            
            _matches[id] = null;
            return true;
        }
        
        public void AddPlayer(OnlineUser self) =>
            _clientsInLobby.TryAdd(self.UserInfo.UserID, self);

        public void RemovePlayer(OnlineUser self) =>
            _clientsInLobby.Remove(self.UserInfo.UserID);
        
        public void BroadcastPacket(BanchoPacket packet)
        {
            foreach (var client in _clientsInLobby.Values)
                client.Connection.SendPacket(packet);
        }
    }*/
}