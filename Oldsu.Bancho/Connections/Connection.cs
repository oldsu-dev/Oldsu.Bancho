using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fleck;

using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Connections
{
    public abstract class Connection
    {
        protected IWebSocketConnection RawConnection { get; }
        public IWebSocketConnectionInfo ConnectionInfo => RawConnection.ConnectionInfo;
        
        private DateTime _pingTimeoutWindow = DateTime.MinValue;

        public bool IsZombie => _disconnectRequest;

        public bool PingTimeout => DateTime.Now > _pingTimeoutWindow;

        public event EventHandler? Disconnected;
        
        protected TaskCompletionSource? LockStateHolder;

        protected Task WaitStateLock()
        {
            var lockStateHolder = LockStateHolder;
            
            return lockStateHolder?.Task ?? Task.CompletedTask;
        }

        public void LockState() => LockStateHolder = new TaskCompletionSource();
        
        public void UnlockState()
        {
             LockStateHolder!.SetResult();
             LockStateHolder = null;
        }

        public Version Version { get;  set; } 
        public Guid Guid { get; }

        public string IP => ConnectionInfo.Headers.TryGetValue("CF-Connecting-IP", out var ip)
            ? ip
            : ConnectionInfo.ClientIpAddress;

        public Connection(Guid guid, IWebSocketConnection webSocketConnection, int pingInterval)
        {
            RawConnection = webSocketConnection;
            Guid = guid;
            RawConnection.OnClose += HandleDisconnection;

            _sendPacketSemaphore = new SemaphoreSlim(1, 1);
            ResetPing(pingInterval);

            _sendingCompletionSource = new TaskCompletionSource();
        }

        protected void ResetPing(int nextPeriod) =>
            _pingTimeoutWindow = DateTime.Now + new TimeSpan(0,0,0,0, nextPeriod);

        private readonly SemaphoreSlim _sendPacketSemaphore;
        
        public Task SendPacketAsync(BanchoPacket packet) => Task.Factory.StartNew(() => InternalSendPacket(packet));
        
        private readonly TaskCompletionSource _sendingCompletionSource;

        private void CheckCompletionState()
        {
            if (_sendingCompleted && _waitingPackets == 0)
                _sendingCompletionSource.SetResult();
        }

        private volatile bool _sendingCompleted;
        private volatile uint _waitingPackets;

        private void CompleteSending()
        {
            _sendingCompleted = true;
            CheckCompletionState();
        }
        
        public async Task InternalSendPacket(BanchoPacket packet)
        {
            await _sendPacketSemaphore.WaitAsync();
            await WaitStateLock();
            
            if (_sendingCompleted)
                return;

            Interlocked.Increment(ref _waitingPackets);

            try
            {
                if (!RawConnection!.IsAvailable)
                    return;

                var data = packet.GetDataByVersion(this.Version);
                if (data == null || data.Length == 0)
                    return;

                await RawConnection.Send(data);
            }
            catch (ConnectionNotAvailableException exception)
            {
                Debug.WriteLine(exception);
                //Disconnect();
            }
            finally
            {
                _sendPacketSemaphore.Release();
                
                Interlocked.Decrement(ref _waitingPackets);
                CheckCompletionState();
            }
        }

        private volatile bool _disconnectRequest = false;

        /// <summary>
        ///     Disconnects client from the server.
        /// </summary>
        public async void Disconnect()
        {
            await WaitStateLock();
            
            if (_disconnectRequest)
                return;

            _disconnectRequest = true;
            
            CompleteSending();
            await _sendingCompletionSource.Task;
            RawConnection.Close();
            
        }

        public void ForceDisconnect() => HandleDisconnection();

        private async void HandleDisconnection()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
            ClearEventSubscriptions();
        }

        protected virtual void ClearEventSubscriptions()
        {
            RawConnection.OnClose -= HandleDisconnection;
        }
    }
}