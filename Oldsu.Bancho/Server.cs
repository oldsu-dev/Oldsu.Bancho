using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Utils;

namespace Oldsu.Bancho
{
    public class Server
    {
        private readonly WebSocketServer _server;

        public static readonly MultiKeyConcurrentDictionary<uint, string, Client> AuthenticatedClients = new();
        public static readonly ConcurrentDictionary<Guid, Client> Clients = new();

        private static async Task PingWatchdog(CancellationToken ct = default)
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
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address)
        {
            _server = new WebSocketServer(address);

        }

        /// <summary>
        ///     Broadcasting to all clients.
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        public static void BroadcastPacket(BanchoPacket packet)
        {
            using var clients = AuthenticatedClients.Values;
            foreach (var c in clients)
            {
                _ = c.SendPacket(packet);
            }
        }

        /// <summary>
        ///     Broadcasting to other clients. 
        /// </summary>
        /// <param name="packet">Packet to broadcast</param>
        /// <param name="id">Id of client to avoid</param>
        public static void BroadcastPacketToOthers(BanchoPacket packet, uint id)
        {
            using var clients = AuthenticatedClients.Values;
            foreach (var c in clients.Where(u => u.ClientContext!.User.UserID != id))
            {
                _ = c.SendPacket(packet);
            }
        }
        
        /// <summary>
        ///     Send packet to a specific user in the server.
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="username">Username of the user to send the packet to.</param>
        public static void SendPacketToSpecificUser(BanchoPacket packet, string username)
        {
            if (AuthenticatedClients.TryGetValue(username, out var user))
                _ = user.SendPacket(packet);
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
                    var client = new Client();
                    
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