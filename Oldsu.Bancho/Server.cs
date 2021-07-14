using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;

namespace Oldsu.Bancho
{
    public class Server
    {
        private readonly WebSocketServer _server;

        public static readonly ConcurrentDictionary<uint, Client> AuthenticatedClients = new();
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

        public static void BroadcastPacket(BanchoPacket packet)
        {
            foreach (var c in AuthenticatedClients.Values)
            {
                _ = c.SendPacket(packet);
            }
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