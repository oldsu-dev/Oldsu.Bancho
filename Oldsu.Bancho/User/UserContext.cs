
using System;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Providers;
using Oldsu.Utils;

namespace Oldsu.Bancho.User
{
    public class UserSubscriptionManager : IAsyncDisposable, IAsyncObserver<ProviderEvent>
    {
        public event EventHandler<BanchoPacket>? PacketInbound;
        
        private IAsyncDisposable? _userStateUnsubscriber;
        private IAsyncDisposable? _userStreamerUnsubscriber;
        private IAsyncDisposable? _userSpectatorUnsubscriber;

        public async Task SubscribeToUserStateProvider(IUserStateProvider provider)
        {
            await UnsubscribeFromUserStateProvider();
            _userStateUnsubscriber = await provider.Subscribe(this);
        }

        // You don't want to anyway
        public async Task UnsubscribeFromUserStateProvider()
        {
            await (_userStateUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _userStateUnsubscriber = null;
        }

        public async Task SubscribeToStreamerObservable(IStreamerObservable provider)
        {
            await UnsubscribeFromStreamerObservable();
            _userStreamerUnsubscriber = await provider.Subscribe(this);
        }

        public async Task UnsubscribeFromStreamerObservable()
        {
            await (_userStreamerUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _userStreamerUnsubscriber = null;
        }

        public async Task SubscribeToSpectatorObservable(ISpectatorObservable provider)
        {
            await UnsubscribeFromSpectatorObservable();
            _userSpectatorUnsubscriber = await provider.Subscribe(this);
        }

        public async Task UnsubscribeFromSpectatorObservable()
        {
            await (_userSpectatorUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _userSpectatorUnsubscriber = null;
        }

        public Task OnNext(object? sender, ProviderEvent value)
        {
            if (value.DataType == ProviderEventType.BanchoPacket)
                PacketInbound?.Invoke(this, (BanchoPacket)value.Data);

            return Task.CompletedTask;
        }

        public Task OnError(object? sender, Exception? exception)
        {
            throw new NotImplementedException();
        }

        public Task OnCompleted(object? sender)
        {
            return sender switch
            {
                ISpectatorObservable => UnsubscribeFromSpectatorObservable(),
                
                _ => throw new InvalidOperationException("Complete is not supported for this Provider.")
            };
        }
        
        public async ValueTask DisposeAsync()
        {
            await UnsubscribeFromSpectatorObservable();
            await UnsubscribeFromStreamerObservable();
            await UnsubscribeFromUserStateProvider();
        }
    }
    
    public class UserContext : IAsyncDisposable
    {
        public UserContext(uint userId, string username, UserSubscriptionManager subscriptionManager,
            IUserStateProvider userStateProvider, IStreamingProvider streamingProvider)
        {
            UserID = userId;
            Username = username;

            UserStateProvider = userStateProvider;
            StreamingProvider = streamingProvider;
            SubscriptionManager = subscriptionManager;
        }

        public IUserStateProvider UserStateProvider { get; }
        public IStreamingProvider StreamingProvider { get; }
        public UserSubscriptionManager SubscriptionManager { get; }

        // private readonly AsyncRwLockWrapper<GameSpectator> _gameSpectator;
        // private readonly AsyncRwLockWrapper<GameBroadcaster> _gameBroadcaster;

        public uint UserID { get; }
        public string Username { get; }

        public async ValueTask DisposeAsync()
        {
            await SubscriptionManager.DisposeAsync();
        }
    }
}