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
        private TaskCompletionSource? _awaiter;
        
        public Task WaitStateLock()
        {
            var lockStateHolder = _awaiter;
            
            return lockStateHolder?.Task ?? Task.CompletedTask;
        }

        public void LockState() => _awaiter = new TaskCompletionSource();
        
        public void UnlockState()
        {
            _awaiter?.SetResult();
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
        private IWebSocketConnection RawConnection { get; }
        
        public LockStateHolder LockStateHolder { get; private set; }
        public Version Version { get; private set; }
        public ConnectionState State { get; private set; }
        
        public event EventHandler? Disconnected;
        public event EventHandler? Ready;
        public event EventHandler<ISharedPacketIn>? OnPacket;
        public event EventHandler<string>? OnString;
        public event EventHandler? Timedout;

        public IWebSocketConnectionInfo ConnectionInfo => RawConnection.ConnectionInfo;
        public bool IsZombie => _disconnectRequest;
        public Guid Guid { get; }

        public string IP => ConnectionInfo.Headers.TryGetValue("CF-Connecting-IP", out var ip)
            ? ip
            : ConnectionInfo.ClientIpAddress;

        public Connection(IWebSocketConnection webSocketConnection)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            RawConnection = webSocketConnection;
            
            RawConnection.OnBinary += HandleBinary;
            RawConnection.OnClose += ForceDisconnect;
            RawConnection.OnMessage += HandleMessage;
            RawConnection.OnOpen += OnOpen;

            State = ConnectionState.Unauthenticated;
            Version = Version.NotApplicable;
            Guid = Guid.NewGuid();

            _enqueuePackets = Channel.CreateUnbounded<ISerializable>();

            _sendLock = new AsyncLock();
            _receiveLock = new AsyncLock();
            
            LockStateHolder = new LockStateHolder();
        }
        
        private void OnOpen()
        {
            if (_disconnectRequest)
                return;
            
            StartSendingPackets();
            
            Ready?.Invoke(this, EventArgs.Empty);
            
            ResetPing(UnauthenticatedPingInterval);
        }

        private async void HandleMessage(string message)
        {
            await LockStateHolder.WaitStateLock();
            
            if (_disconnectRequest)
                return;
            
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
            
            if (_disconnectRequest)
                return;

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

        private CancellationTokenSource? _pingTaskCancellationSource;
        private CancellationTokenSource? _packetSendTaskCancellationSource;
        
        private void ResetPing(int nextPeriod)  
        {
            _pingTaskCancellationSource?.Cancel();
            _pingTaskCancellationSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                await Task.Delay(nextPeriod, _pingTaskCancellationSource.Token);
                Timedout?.Invoke(this, EventArgs.Empty);
                await Disconnect(true);
            });
        }

        private void StartSendingPackets()
        {
            StopSendingPackets();
            
            _packetSendTaskCancellationSource = new CancellationTokenSource();
            Task.Run(PacketSendTask);
        }
        
        private void StopSendingPackets()
        {
            _packetSendTaskCancellationSource?.Cancel();
            _packetSendTaskCancellationSource = null;
        }

        private void CancelPing() =>
            _pingTaskCancellationSource?.Cancel();
        
        private readonly AsyncLock _sendLock;
        private readonly AsyncLock _receiveLock;

        public void SendPacket(ISerializable packet) => _enqueuePackets.Writer.TryWrite(packet);

        public void Authenticate(Version version)
        {
            State = ConnectionState.Authenticated;
            Version = version;
            ResetPing(AuthenticatedPingInterval);
        }

        private readonly Channel<ISerializable> _enqueuePackets;

        private async Task PacketSendTask()
        {
            CancellationToken token = _packetSendTaskCancellationSource?.Token ?? default;
            
            for (;;)
            {
                ISerializable packet = await _enqueuePackets.Reader.ReadAsync(token);

                try
                {
                    if (!RawConnection.IsAvailable)
                        return;

                    var data = packet.SerializeDataByVersion(this.Version);
                    if (data == null || data.Value.Length == 0)
                        return;

                    Console.WriteLine(packet.ToString());

                    await RawConnection.Send(data.Value);
                }
                catch (ConnectionNotAvailableException exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        
        private volatile bool _disconnectRequest;

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
            _enqueuePackets.Writer.Complete();
            
            if (!force)
            {
                await _enqueuePackets.Reader.Completion;
            }
            
            RawConnection.Close();
            HandleDisconnection();
        }

        private void HandleDisconnection()
        {
            if (!_disconnectRequest)
                _disconnectRequest = true;
         
            Disconnected?.Invoke(this, EventArgs.Empty);
            
            StopSendingPackets();
            CancelPing();
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
            RawConnection.OnMessage -= HandleMessage;
            RawConnection.OnOpen -= OnOpen;
        }
    }
}