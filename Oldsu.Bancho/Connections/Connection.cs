using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Fleck;
using Nito.AsyncEx;
using Oldsu.Bancho.Packet;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Connections
{
    public class LockStateHolder
    {
        private TaskCompletionSource? _awaiter { get; set; }
        
        public Task WaitStateLock()
        {
            var lockStateHolder = _awaiter;
            
            return lockStateHolder?.Task ?? Task.CompletedTask;
        }

        public void LockState() => _awaiter = new TaskCompletionSource();
        
        public void UnlockState()
        {
            _awaiter!.SetResult();
            _awaiter = null;
        }
    }

    public enum ConnectionState
    {
        Unauthenticated,
        WaitingAuthentication,
        Authenticated
    }
    
    public class Connection : IAsyncDisposable
    {
        protected IWebSocketConnection RawConnection { get; }
        
        public LockStateHolder LockStateHolder { get; protected set; }
        public Version Version { get; private set; }
        public ConnectionState State { get; private set; }
        public Guid Guid { get; }
        
        public event EventHandler? Disconnected;
        public event EventHandler? Ready;
        public event EventHandler<ISharedPacketIn>? OnPacket;
        public event EventHandler<string>? OnString;

        public IWebSocketConnectionInfo ConnectionInfo => RawConnection.ConnectionInfo;
        public bool IsZombie => _disconnectRequest;
        public bool IsTimedout => DateTime.Now > _pingTimeoutWindow;
        
        private DateTime _pingTimeoutWindow = DateTime.MinValue;

        public string IP => ConnectionInfo.Headers.TryGetValue("CF-Connecting-IP", out var ip)
            ? ip
            : ConnectionInfo.ClientIpAddress;

        public Connection(Guid guid, IWebSocketConnection webSocketConnection)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            RawConnection = webSocketConnection;
            Guid = guid;
            
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnClose += ForceDisconnect;
            RawConnection.OnMessage += HandleMessage;
            RawConnection.OnOpen += OnOpen;

            ResetPing(UnauthenticatedPingInterval);

            State = ConnectionState.Unauthenticated;
            Version = Version.NotApplicable;

            _sendingCompletionSource = new TaskCompletionSource();
            
            _sendLock = new AsyncLock();
            _receiveLock = new AsyncLock();
            
            LockStateHolder = new LockStateHolder();
        }

        private void OnOpen()
        {
            Ready?.Invoke(this, EventArgs.Empty);
        }

        private async void HandleMessage(string message)
        {
            await LockStateHolder.WaitStateLock();
            
            using (await _receiveLock.LockAsync())
            {
                switch (State)
                {
                    case ConnectionState.Unauthenticated:
                        OnString?.Invoke(this, message);
                        
                        State = ConnectionState.WaitingAuthentication;
                        LockStateHolder.LockState();
                        
                        break;
                    default:
                        await Disconnect(true);
                        break;
                }
            }
        }

        private const int UnauthenticatedPingInterval = 10_000;
        private const int AuthenticatedPingInterval = 35_000;
        
        private async void HandleBinary(byte[] data)
        {
            await LockStateHolder.WaitStateLock();

            using (await _receiveLock.LockAsync())
            {
                switch (State)
                {
                    case ConnectionState.Authenticated:
                        ResetPing(AuthenticatedPingInterval);

                        
                        var obj = BanchoSerializer.Deserialize(data, this.Version);
                        if (obj == null)
                        {
                            return;
                        }

                        ISharedPacketIn packet = ((IntoPacket<ISharedPacketIn>) obj).IntoPacket();
                        
                        Debug.WriteLine(packet);
                        
                        OnPacket?.Invoke(this, packet);
                        break;
                    default:
                        await Disconnect(true);
                        break;
                }
            }
        }

        protected void ResetPing(int nextPeriod) =>
            _pingTimeoutWindow = DateTime.Now + new TimeSpan(0,0,0,0, nextPeriod);

        private readonly AsyncLock _sendLock;
        private readonly AsyncLock _receiveLock;

        public async void SendPacket(ISerializable packet) => await InternalSendPacket(packet);
        
        private readonly TaskCompletionSource _sendingCompletionSource;

        private void CheckCompletionState()
        {
            if (_sendingCompleted && _waitingPackets == 0)
                _sendingCompletionSource.TrySetResult();
        }

        private volatile bool _sendingCompleted;
        private volatile uint _waitingPackets;

        private void CompleteSending()
        {
            _sendingCompleted = true;
            CheckCompletionState();
        }

        public void Authenticate(Version version)
        {
            State = ConnectionState.Authenticated;
            Version = version;
        }
        
        public async Task InternalSendPacket(ISerializable packet)
        {
            await LockStateHolder.WaitStateLock();

            Interlocked.Increment(ref _waitingPackets);

            using (await _sendLock.LockAsync())
            {
                if (_sendingCompleted)
                    return;

                try
                {
                    if (!RawConnection.IsAvailable)
                        return;

                    var data = packet.SerializeDataByVersion(this.Version);
                    if (data == null || data.Value.Length == 0)
                        return;

                    await RawConnection.Send(data.Value);
                }
                catch (ConnectionNotAvailableException exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    Interlocked.Decrement(ref _waitingPackets);
                    CheckCompletionState();
                }
            }
        }

        private volatile bool _disconnectRequest = false;

        public async void ForceDisconnect()
        {
            await Disconnect(true);
        }
        
        /// <summary>
        ///     Disconnects client from the server.
        /// </summary>
        public async Task Disconnect(bool force)
        {
            await LockStateHolder.WaitStateLock();
            
            if (_disconnectRequest)
                return;

            _disconnectRequest = true;
            
            CompleteSending();
            
            if (!force)
            {
                try
                {
                    await _sendingCompletionSource.Task;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
            
            _sendingCompletionSource.TrySetException(new TaskCanceledException());
            
            RawConnection.Close();
            HandleDisconnection();
        }

        private void HandleDisconnection()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private readonly CancellationTokenSource _cancellationTokenSource;

        public void StopReceiving()
        {
            _cancellationTokenSource.Cancel();
        }
        
        public async ValueTask DisposeAsync()
        {            
            await Disconnect(true);
            RawConnection.OnBinary -= HandleBinary;
            RawConnection.OnClose -= ForceDisconnect;
        }
    }
}