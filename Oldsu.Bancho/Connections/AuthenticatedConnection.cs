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
    public class AuthenticatedConnection : Connection, IAsyncObserver<BanchoPacket>
    {
        public const int PingMaxInterval = 35_000;

        private readonly IDisposable _banchoPacketObserverUnsubscriber;
        
        public ConnectedUserContext ConnectedUserContext { get; }

        public AuthenticatedConnection(Guid guid, IWebSocketConnection webSocketConnection, UserContext userContext) 
            : base(guid, webSocketConnection, PingMaxInterval)
        {
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnMessage += HandleMessage;

            ConnectedUserContext = userContext.Connect(this);

            _banchoPacketObserverUnsubscriber = ConnectedUserContext.UserDataProvider.SubscribeAsync(this);
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
            packet.Handle(ConnectedUserContext);
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
                _banchoPacketObserverUnsubscriber.Dispose();
                
                RawConnection.OnBinary -= HandleBinary;
                RawConnection.OnMessage -= HandleMessage;
            }
        }

        Task IAsyncObserver<BanchoPacket>.OnNextAsync(BanchoPacket statusUpdate) =>
            this.SendPacketAsync(statusUpdate);

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