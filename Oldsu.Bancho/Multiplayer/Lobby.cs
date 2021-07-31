using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Multiplayer
{
    public class Lobby
    {
        public const int MatchesAvailable = 256;
        
        private readonly Dictionary<uint, OnlineUser> _clientsInLobby = new();
        private readonly AsyncRwLockWrapper<Match?>[] _matches = new AsyncRwLockWrapper<Match?>[MatchesAvailable];

        public Lobby()
        {
            for (int i = 0; i < MatchesAvailable; i++)
                _matches[i] = new AsyncRwLockWrapper<Match?>();
        }
        
        public async Task<AsyncRwLockWrapper<Match>?> RegisterMatchAsync(Match match)
        {
            for (byte i = 0; i <= (MatchesAvailable - 1); i++)
                if (_matches[i].IsNull)
                {
                    await _matches[i].SetValueAsync(match);
                    match.MatchID = i;
                    return _matches[i]!;
                }

            return null;
        }

        public async Task<bool> DisbandMatchAsync(int id, OnlineUser self)
        {
            if (self.UserInfo.UserID != await _matches[id].ReadAsync(match => match?.HostID ?? null)) 
                return false;
            
            await _matches[id].SetValueAsync(null);
            return true;
        }
        
        public void AddPlayer(OnlineUser self) =>
            _clientsInLobby.TryAdd(self.UserInfo.UserID, self);

        public void RemovePlayer(OnlineUser self) =>
            _clientsInLobby.Remove(self.UserInfo.UserID);

        public void BroadcastPacketToPlayersInLobby(BanchoPacket packet)
        {
            foreach (var client in _clientsInLobby.Values)
                client.Connection.SendPacket(packet);
        }
    }
}