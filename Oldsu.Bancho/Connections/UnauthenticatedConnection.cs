using System;
using System.Threading.Tasks;
using Fleck;

using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Connections
{
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
        
        private async void HandleBinary(byte[] data)
        {
            await LockStateHolder.WaitStateLock();
            // UnauthenticatedConnection can't receive data messages 
            Disconnect();
        }

        private volatile bool _loginReceived;
        
        private async void HandleMessage(string message)
        {
            await LockStateHolder.WaitStateLock();
            
            if (_loginReceived)
                return;

            _loginReceived = true;
            
            Login?.Invoke(this, message);
        }

        protected override void ClearEventSubscriptions()
        {
            base.ClearEventSubscriptions();
            
            RawConnection.OnBinary -= HandleBinary;
            RawConnection.OnMessage -= HandleMessage;
        }

        public async Task<AuthenticatedConnection> Upgrade(Version version)
        {
            ClearEventSubscriptions();

            var authenticatedConnection = new AuthenticatedConnection(Guid, RawConnection, LockStateHolder);
            authenticatedConnection.Version = version;

            return authenticatedConnection;
        }
    }
}