using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using WebSocketException = Fleck.WebSocketException;

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
        ///     Starts a websocket server and listening to incoming requests
        /// </summary>
        public async Task Start()
        {
            try
            {
                _server.Start(socket =>
                {
                    socket.OnMessage = async message => await (new Client(socket)).HandleLoginAsync(message);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}