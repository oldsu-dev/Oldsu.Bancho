using System;
using System.Threading.Tasks;
using Fleck;

namespace Oldsu.Bancho
{
    public class Server
    {
        private WebSocketServer _server;

        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address)
        {
            _server = new WebSocketServer(address);
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleLoginAsync on client login.
        /// </summary>
        public async Task Start()
        {
            try
            {
                _server.Start(socket =>
                {
                    new Client().BindWebSocket(socket);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}