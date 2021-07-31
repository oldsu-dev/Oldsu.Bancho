using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Packet;

using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho {
    /// <summary>
    ///     Class for each user in oldsu's bancho
    /// </summary>
    public abstract class Connection : IDisposable
    {
        protected IWebSocketConnection RawConnection { get; }
        public IWebSocketConnectionInfo ConnectionInfo => RawConnection.ConnectionInfo;
        
        private DateTime _pingTimeoutWindow = DateTime.MinValue;
        
        public bool PingTimeout => DateTime.Now > _pingTimeoutWindow;

        public event EventHandler? Disconnected;

        public Version Version { get;  set; } 
        public Guid Guid { get; }
        
        public Connection(Guid guid, IWebSocketConnection webSocketConnection, int pingInterval)
        {
            RawConnection = webSocketConnection;
            Guid = guid;
            RawConnection.OnClose += HandleDisconnection;

            ResetPing(pingInterval);
        }

        protected void ResetPing(int nextPeriod)
        { 
            _pingTimeoutWindow = DateTime.Now + new TimeSpan(0,0,0,0, nextPeriod);
        }
        
        private readonly SemaphoreSlim _sendPacketSemaphore = new(1, 1);

        public void SendPacket(BanchoPacket packet) => Task.Run(() => SendPacketAsync(packet));

        /// <summary>
        ///     Sends packet to client.
        /// </summary>
        /// <param name="packet"> Packet meant to be sent. </param>
        public async Task SendPacketAsync(BanchoPacket packet)
        {
            await _sendPacketSemaphore.WaitAsync();

            try
            {
                if (!RawConnection!.IsAvailable)
                    return;

                var data = packet.GetDataByVersion(this.Version);

                if (data.Length == 0)
                    return;

                await RawConnection!.Send(data);
            }
            catch (ConnectionNotAvailableException exception)
            {
                Debug.WriteLine(exception);
                //Disconnect();
            }
            finally
            {
                _sendPacketSemaphore.Release();
            }
        }

        /// <summary>
        ///     Disconnects client from the server.
        /// </summary>
        public void Disconnect()
        {
            RawConnection.Close();
        }

        private void HandleDisconnection() =>
            Disconnected?.Invoke(this, EventArgs.Empty);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sendPacketSemaphore.Dispose();
                RawConnection.OnClose -= HandleDisconnection;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class UnauthenticatedConnection : Connection
    {
        public event EventHandler<string>? Login;

        public const int AuthMaxInterval = 10_000;

        
        public UnauthenticatedConnection(Guid guid, IWebSocketConnection webSocketConnection) 
            : base(guid, webSocketConnection, AuthMaxInterval)
        {
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnMessage += HandleMessage;
        }
        
        private void HandleBinary(byte[] data)
        {
            // UnauthenticatedConnection can't receive data messages 
            Disconnect();
        }
        
        private void HandleMessage(string message) =>
            Login?.Invoke(this, message);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                RawConnection.OnBinary -= HandleBinary;
                RawConnection.OnMessage -= HandleMessage;
            }
        }

        public AuthenticatedConnection Upgrade(Version version)
        {
            var authenticatedConnection = new AuthenticatedConnection(Guid, RawConnection);

            authenticatedConnection.Version = version;
            Dispose();

            return authenticatedConnection;
        }
    }
    
    public class AuthenticatedConnection : Connection
    {
        public event EventHandler<ISharedPacketIn>? PacketReceived;
        
        public const int PingMaxInterval = 35_000;
        
        public AuthenticatedConnection(Guid guid, IWebSocketConnection webSocketConnection) 
            : base(guid, webSocketConnection, PingMaxInterval)
        {
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnMessage += HandleMessage;
        }
        
        private void HandleBinary(byte[] data)
        {
            ResetPing(PingMaxInterval);
            
            var obj = BanchoSerializer.Deserialize(data, this.Version);
            if (obj == null)
            {
                return;
            }
            
            ISharedPacketIn packet = ((Into<ISharedPacketIn>)obj).Into();
            PacketReceived?.Invoke(this, packet);
        }

        private void HandleMessage(string message)
        {
            // UnauthenticatedConnection can't receive text messages 
            Disconnect();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                RawConnection.OnBinary -= HandleBinary;
                RawConnection.OnMessage -= HandleMessage;
            }
        }
    }
}