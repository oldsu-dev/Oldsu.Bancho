using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Handshakes;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Utils;

namespace Oldsu.Bancho.Connections
{
    public class ConnectionEventHandler
    {
        public ConnectionEventHandler(UserContext userContext, Connection connection)
        {
            _userContext = userContext;
            _connection = connection;
            _eventSemaphore = new SemaphoreSlim(1,1);
        }
        
        private UserContext _userContext;
        private Connection _connection;
        private SemaphoreSlim _eventSemaphore;
        
        public async void ProcessPacket(ISharedPacketIn packet)
        {
            await _eventSemaphore.WaitAsync();

            try
            {
                await packet.Handle(_userContext, _connection);
            }
            finally
            {
                _eventSemaphore.Release();
            }
        }
        
        public void PacketInbound(BanchoPacket packet)
        {
            _connection.SendPacket(packet);
        }

        public async void UserRequestInbound(UserRequestTypes request)
        {
            await _eventSemaphore.WaitAsync();

            try
            {
                await _userContext.HandleUserRequest(request);
            }
            finally
            {
                _eventSemaphore.Release();
            }
        }

        public async void DisposeUserContext()
        {
            await _eventSemaphore.WaitAsync();

            try
            {
                await _userContext.DisposeAsync();
            }
            finally
            {
                _eventSemaphore.Release();
            }
        }
    }

    public class AuthenticatedConnection : Connection
    {
        
        public const int PingMaxInterval = 35_000;

        public event EventHandler<ISharedPacketIn>? PacketReceived;
        
        public AuthenticatedConnection(Guid guid, IWebSocketConnection webSocketConnection) 
            : base(guid, webSocketConnection, PingMaxInterval)
        {
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnMessage += HandleMessage;
        }

        public Task SendHandshake(IHandshake handshake) => handshake.Execute(this);
        
        private void HandleBinary(byte[] data)
        {
            ResetPing(PingMaxInterval);
            
            var obj = BanchoSerializer.Deserialize(data, this.Version);
            if (obj == null)
            {
                return;
            }

            ISharedPacketIn packet = ((IntoPacket<ISharedPacketIn>)obj).IntoPacket();
            Debug.WriteLine(packet.GetType());
            
            PacketReceived?.Invoke(this, packet);
        }

        private void HandleMessage(string message)
        {
            // UnauthenticatedConnection can't receive text messages 
            Disconnect();
        }

        protected override void ClearEventSubscriptions()
        {
            base.ClearEventSubscriptions();
            
            RawConnection.OnBinary -= HandleBinary;
            RawConnection.OnMessage -= HandleMessage;
        }
    }
}