using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Types;
using Oldsu.Utils;

namespace Oldsu.Bancho
{
    public class Server
    {
        private readonly WebSocketServer _server;

        public readonly MultiKeyConcurrentDictionary<uint, string, Client> AuthenticatedClients = new();
        public readonly ConcurrentDictionary<Guid, Client> Clients = new();

        public readonly Lobby MultiplayerLobby = new Lobby();

        public static Channel[] Channels { get; set; }

        private async Task PingWatchdog(CancellationToken ct = default)
        {
            for (;;)
            {
                ct.ThrowIfCancellationRequested();
                
                foreach (var client in Clients.Values.Where(c => DateTime.Now > c.PingTimeoutWindow))
                    client.Disconnect();
                
                await Task.Delay(1000, ct);
            }
        }

        /// <summary>
        ///     Initializes the server. Fetches all the channels in the database, and sets them into a static variable.
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address)
        {
            if (Channels == null)
            {
                var db = new Database();
                Channels = db.AvailableChannels.ToArray();
            }

            _server = new WebSocketServer(address);
        }

        /// <summary>
        ///     Broadcasting to all clients.
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        public void BroadcastPacket(BanchoPacket packet)
        {
            using var clients = AuthenticatedClients.Values;
            foreach (var c in clients)
            {
                c.SendPacket(packet);
            }
        }

        /// <summary>
        ///     Broadcasting to other clients. 
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        /// <param name="id">Id of client to avoid</param>
        public void BroadcastPacketToOthers(BanchoPacket packet, uint id)
        {
            using var clients = AuthenticatedClients.Values;
            foreach (var c in clients.Where(u => u.ClientContext!.User.UserID != id))
            {
                c.SendPacket(packet);
            }
        }
        
        /// <summary>
        ///     Send packet to a specific user in the server.
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="username">Username of the user to send the packet to.</param>
        public void SendPacketToSpecificUser(BanchoPacket packet, string username)
        {
            if (AuthenticatedClients.TryGetValue(username, out var user))
                user.SendPacket(packet);
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleLoginAsync on client login.
        /// </summary>
        public async Task Run(CancellationToken ct = default)
        {
            await Task.Factory.StartNew(() => PingWatchdog(ct), ct);
            
            try
            {
                _server.Start(socket =>
                {
                    var client = new Client(this);
                    
                    client.BindWebSocket(socket);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(-1, ct);
        }
    }
}