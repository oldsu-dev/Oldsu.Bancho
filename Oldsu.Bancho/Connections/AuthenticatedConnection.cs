using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Exceptions;
using Oldsu.Bancho.Handshakes;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Logging;
using Oldsu.Utils;

namespace Oldsu.Bancho.Connections
{
    public class ConnectionEventHandler
    {
        private readonly LoggingManager _loggingManager;
        
        public ConnectionEventHandler(LoggingManager loggingManager, UserContext userContext, Connection connection)
        {
            _userContext = userContext;
            _connection = connection;
            _eventSemaphore = new SemaphoreSlim(1,1);
            _loggingManager = loggingManager;
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
            catch (OldsuException oldsuException)
            {
                await _loggingManager.LogCritical<ConnectionEventHandler>(
                    "Thrown exception when handling packet. User was disconnected", 
                    oldsuException, 
                    new
                    {
                        _userContext.Username,
                        _userContext.UserID,
                        _userContext.Privileges,
                    });
                
                _connection.Disconnect();
            }
            finally
            {
                _eventSemaphore.Release();
            }
        }
        
        public void PacketInbound(BanchoPacket packet)
        {
            _connection.SendPacketAsync(packet);
        }

        public async void UserRequestInbound(UserRequest request)
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

        public async Task DisposeUserContext()
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
        
        internal AuthenticatedConnection(Guid guid, IWebSocketConnection webSocketConnection, LockStateHolder lockStateHolder) 
            : base(guid, webSocketConnection, PingMaxInterval)
        {
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnMessage += HandleMessage;

            LockStateHolder = lockStateHolder;
        }

        public Task SendHandshake(IHandshake handshake) => handshake.Execute(this);
        
        private async void HandleBinary(byte[] data)
        {
            await LockStateHolder.WaitStateLock();
            
            ResetPing(PingMaxInterval);
            
            var obj = BanchoSerializer.Deserialize(data, this.Version);
            if (obj == null)
            {
                return;
            }

            ISharedPacketIn packet = ((IntoPacket<ISharedPacketIn>)obj).IntoPacket();
            
            PacketReceived?.Invoke(this, packet);
        }

        private async void HandleMessage(string message)
        {
            await LockStateHolder.WaitStateLock();
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