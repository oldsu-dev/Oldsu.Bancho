using System;
using System.Threading.Tasks;
using Fleck;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Handshakes;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Utils;

namespace Oldsu.Bancho.Connections
{
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

        public void SendHandshake(IHandshake handshake) => handshake.Execute(this);
        
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
        
        public Task OnErrorAsync(Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task OnCompletedAsync()
        {
            throw new NotImplementedException();
        }
    }
}