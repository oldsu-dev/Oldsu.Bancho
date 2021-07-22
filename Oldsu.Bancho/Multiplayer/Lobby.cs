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
        
        private readonly Dictionary<uint, Client> _clientsInLobby = new();

        private AsyncRwLockWrapper<Match>?[] _matches = new AsyncRwLockWrapper<Match>?[MatchesAvailable];

        public Server Server { get; }
        
        public Lobby(Server server)
        {
            Server = server;
        }
        
        public bool RegisterMatch(Match match, out AsyncRwLockWrapper<Match> matchWrapper)
        {
            matchWrapper = new AsyncRwLockWrapper<Match>(match);
            
            for (byte i = 0; i <= (MatchesAvailable - 1); i++)
                if (_matches[i] == null)
                {
                    _matches[i] = matchWrapper;
                    match.MatchID = i;
                    return true;
                }

            return false;
        }

        public async Task<bool> DisbandMatch(int id, int clientId)
        {
            if (_matches[id] == null || clientId != await _matches[id]!.ReadAsync(host => host.HostID)) 
                return false;
            
            _matches[id] = null;
            return true;
        }
        
        public async Task AddPlayerAsync(Client client)
        {
            _clientsInLobby.TryAdd(await client.GetUserID(), client);
        }

        public async Task RemovePlayerAsync(Client client)
        {
            _clientsInLobby.Remove(await client.GetUserID(), out _);
        }

        public void BroadcastPacketToPlayersInLobby(BanchoPacket packet)
        {
            foreach (var client in _clientsInLobby.Values)
            {
                client.SendPacket(packet);
            }
        }
    }
}