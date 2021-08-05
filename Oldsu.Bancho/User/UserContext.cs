
using System;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Providers;
using Oldsu.Types;
using Oldsu.Utils;
using Action = Oldsu.Enums.Action;

namespace Oldsu.Bancho.User
{
    public class UserSubscriptionManager : IAsyncDisposable, IAsyncObserver<ProviderEvent>
    {
        public event EventHandler<BanchoPacket>? PacketInbound;
        
        private IAsyncDisposable? _userStateUnsubscriber;
        private IAsyncDisposable? _userStreamerUnsubscriber;
        private IAsyncDisposable? _userSpectatorUnsubscriber;
        private IAsyncDisposable? _matchUpdateUnsubscriber;

        public async Task SubscribeToMatchUpdates(ILobbyProvider provider)
        {
            await UnsubscribeFromMatchUpdates();
            _matchUpdateUnsubscriber = await provider.Subscribe(this);
        }
        
        public async Task SubscribeToMatchUpdates(IMatchObservable provider)
        {
            await UnsubscribeFromMatchUpdates();
            _matchUpdateUnsubscriber = await provider.Subscribe(this);
        }

        // You don't want to anyway
        public async Task UnsubscribeFromMatchUpdates()
        {
            await (_matchUpdateUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _matchUpdateUnsubscriber = null;
        }
        
        public async Task SubscribeToUserStateProvider(IUserStateProvider provider)
        {
            await UnsubscribeFromUserStateProvider();
            _userStateUnsubscriber = await provider.Subscribe(this);
        }

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
            return Task.CompletedTask; 
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
            await UnsubscribeFromMatchUpdates();
        }
    }
    
    public class UserContext : IAsyncDisposable
    {
        public UserContext(
            uint userId, string username, 
            IUserStateProvider userStateProvider, 
            IStreamingProvider streamingProvider,
            ILobbyProvider lobbyProvider)
        {
            UserID = userId;
            Username = username;

            SubscriptionManager = new UserSubscriptionManager();
            UserStateProvider = userStateProvider;
            StreamingProvider = streamingProvider;
            LobbyProvider = lobbyProvider;
        }

        public async Task InitialRegistration(UserInfo userInfo, Presence presence)
        {
            await SubscriptionManager.SubscribeToUserStateProvider(UserStateProvider);
            await StreamingProvider.RegisterStreamer(UserID);

            await SubscriptionManager.SubscribeToStreamerObservable(
                await StreamingProvider.GetStreamerObserver(UserID));

            await using Database database = new Database();
            
            await UserStateProvider.RegisterUserAsync(userInfo!.UserID,
                new UserData()
                {
                    Activity = new Activity {Action = Action.Idle},
                    Presence = presence,
                    Stats = await database.GetStatsWithRankAsync(userInfo.UserID, 0),
                    UserInfo = userInfo!
                });
        }
        
        public IUserStateProvider UserStateProvider { get; }
        public IStreamingProvider StreamingProvider { get; }
        public ILobbyProvider LobbyProvider { get; }
        public UserSubscriptionManager SubscriptionManager { get; }

        // private readonly AsyncRwLockWrapper<GameSpectator> _gameSpectator;
        // private readonly AsyncRwLockWrapper<GameBroadcaster> _gameBroadcaster;

        public uint UserID { get; }
        public string Username { get; }

        public async ValueTask DisposeAsync()
        {
            await StreamingProvider.UnregisterStreamer(UserID);
            await UserStateProvider.UnregisterUserAsync(UserID);
            await SubscriptionManager.DisposeAsync();
        }
    }
}