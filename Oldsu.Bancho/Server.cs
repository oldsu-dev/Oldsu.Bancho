using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Utils;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho
{
    public class Server
    {
        private readonly WebSocketServer _server;

        public AsyncRwLockWrapper<MultiKeyDictionary<uint, string, Client>> AuthenticatedClients { get; } = 
            new(new MultiKeyDictionary<uint, string, Client>());
        public AsyncRwLockWrapper<Dictionary<Guid, Client>> Clients { get; } = 
            new(new Dictionary<Guid, Client>());

        public AsyncRwLockWrapper<Lobby> MultiplayerLobby { get; }

        private async Task PingWatchdog(CancellationToken ct = default)
        {
            for (;;)
            {
                ct.ThrowIfCancellationRequested();

                await Clients.ReadAsync(async (clients) =>
                {
                    foreach (var client in clients.Values.Where(c => DateTime.Now > c.PingTimeoutWindow))
                        await client.DisconnectAsync();
                });
                
                await Task.Delay(1000, ct);
            }
        }

        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address)
        {
            _server = new WebSocketServer(address);
            MultiplayerLobby = new(new Lobby(this));
        }

        /// <summary>
        ///     Broadcasting to all clients.
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        public async Task BroadcastPacketAsync(BanchoPacket packet)
        {
            await AuthenticatedClients.ReadAsync((clients) =>
            {
                foreach (var c in clients.Values)
                    c.SendPacket(packet);
            });
        }

        /// <summary>
        ///     Broadcasting to other clients. 
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        /// <param name="id">Id of client to avoid</param>
        public async Task BroadcastPacketToOthersAsync(BanchoPacket packet, uint id)
        {
            await AuthenticatedClients.ReadAsync(async clients =>
            {
                foreach (var c in clients.Values)
                {
                    if (await c.GetUserID() == id)
                        continue;
                    
                    c.SendPacket(packet);
                }
            });
        }
        
        /// <summary>
        ///     Send packet to a specific user in the server.
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="username">Username of the user to send the packet to.</param>
        public async Task<bool> SendPacketToSpecificUserAsync(BanchoPacket packet, string username)
        {
            return await AuthenticatedClients.ReadAsync((clients) =>
            {
                bool hasClient = clients.TryGetValue(username, out var user);
                
                if (hasClient)
                    user.SendPacket(packet);

                return hasClient;
            });
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
                _server.Start(async socket =>
                {
                    var client = new Client(this);
                    await client.BindWebSocketAsync(socket);
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