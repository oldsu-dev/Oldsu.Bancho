using System;
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
}