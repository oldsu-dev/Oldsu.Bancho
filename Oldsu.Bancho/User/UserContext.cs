
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Types;
using Oldsu.Utils;
using Action = Oldsu.Enums.Action;
using ChannelLeft = Oldsu.Bancho.Packet.Shared.Out.ChannelLeft;

namespace Oldsu.Bancho.User
{
    public class UserSubscriptionManager : IAsyncDisposable, IAsyncObserver<ProviderEvent>
    {
        public event EventHandler<BanchoPacket>? PacketInbound;
        public event EventHandler<ProviderEvent>? EventInbound;

        private IAsyncDisposable? _userStateUnsubscriber;
        private IAsyncDisposable? _userStreamerUnsubscriber;
        private IAsyncDisposable? _userSpectatorUnsubscriber;
        private IAsyncDisposable? _matchUpdateUnsubscriber;
        private IAsyncDisposable? _userRequestUnsubscriber;

        private IAsyncDisposable? _chatUnsubscriber;
        
        public Dictionary<string, IAsyncDisposable> _channelUnsubscribers = new();

        public bool SubscribedToLobby { get; private set; } = false;

        public async Task SubscribeToChat(IChatProvider provider)
        {
            await UnsubscribeFromChat();
            _chatUnsubscriber = await provider.Subscribe(this);
        }

        public async Task UnsubscribeFromChat()
        {
            await (_chatUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _chatUnsubscriber = null;
        }
        
        public async Task SubscribeToChannel(IChatChannel channel)
        {
            await UnsubscribeFromChannel(channel);
            _channelUnsubscribers.Add(channel.ChannelInfo.Tag, await channel.Subscribe(this));
        }

        public async Task UnsubscribeFromChannel(string tag)
        {
            if (_channelUnsubscribers.Remove(tag, out var unsubscriber))
                await unsubscriber.DisposeAsync();
        }
        
        public async Task UnsubscribeFromChannel(IChatChannel channel)
        {
            if (_channelUnsubscribers.Remove(channel.ChannelInfo.Tag, out var unsubscriber))
                await unsubscriber.DisposeAsync();
        }
        
        public async Task SubscribeToUserRequests(IUserRequestObservable provider)
        {
            await UnsubscribeFromUserRequests();
            _userRequestUnsubscriber = await provider.Subscribe(this);
        }
        
        public async Task UnsubscribeFromUserRequests()
        {
            await (_userRequestUnsubscriber?.DisposeAsync() ?? ValueTask.CompletedTask);
            _userRequestUnsubscriber = null;
        }
        
        public async Task SubscribeToMatchUpdates(ILobbyProvider provider)
        {
            await UnsubscribeFromMatchUpdates();
            SubscribedToLobby = true;
            _matchUpdateUnsubscriber = await provider.Subscribe(this);
        }
        
        public async Task SubscribeToMatchSetupUpdates(IMatchSetupObservable provider)
        {
            await UnsubscribeFromMatchUpdates();
            SubscribedToLobby = false;
            _matchUpdateUnsubscriber = await provider.Subscribe(this);
        }

        public async Task SubscribeToMatchGameUpdates(IMatchGameObservable provider)
        {
            await UnsubscribeFromMatchUpdates();
            SubscribedToLobby = false;
            _matchUpdateUnsubscriber = await provider.Subscribe(this);
        }
        
        // You don't want to anyway
        public async Task UnsubscribeFromMatchUpdates()
        {            
            SubscribedToLobby = false;
            
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
            {
                PacketInbound?.Invoke(this, (BanchoPacket) value.Data!);
                return Task.CompletedTask;
            }

            EventInbound?.Invoke(this, value);
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
            foreach (var channelUnsubscribers in _channelUnsubscribers.Values)
                await channelUnsubscribers.DisposeAsync();

            await UnsubscribeFromChat();
            await UnsubscribeFromSpectatorObservable();
            await UnsubscribeFromStreamerObservable();
            await UnsubscribeFromUserStateProvider();
            await UnsubscribeFromUserRequests();
            await UnsubscribeFromMatchUpdates();
        }
    }
    
    public class UserContext : IAsyncDisposable
    {
        public uint UserID { get; }
        public string Username { get; }
        public Privileges Privileges { get; }

        public DependencyManager Dependencies { get; }
        public LoggingManager Logger { get; }
        
        // public IUserStateProvider UserStateProvider { get; }
        // public IStreamingProvider StreamingProvider { get; }
        // public ILobbyProvider LobbyProvider { get; }
        // public IUserRequestProvider UserRequestProvider { get; }
        // public IChatProvider ChatProvider { get; }
        
        public UserSubscriptionManager SubscriptionManager { get; }

        public UserContext(uint userId, string username, Privileges privileges, 
            DependencyManager dependencies, LoggingManager loggingManager)
        {
            UserID = userId;
            Username = username;
            Privileges = privileges;

            SubscriptionManager = new UserSubscriptionManager();

            Dependencies = dependencies;
            Logger = loggingManager;
            _waitDisconnectionSource = new TaskCompletionSource();
        }

        public async Task HandleUserRequest(UserRequestTypes request)
        {
            switch (request)
            {
                case UserRequestTypes.QuitMatch:
                    await Dependencies.Get<ILobbyProvider>().TryLeaveMatch(UserID);

                    await LeaveChannel("#multiplayer");

                    if (!SubscriptionManager.SubscribedToLobby)
                        await SubscriptionManager.UnsubscribeFromMatchUpdates();

                    break;
                case UserRequestTypes.AnnounceTransferHost:
                    await SubscriptionManager.OnNext(this, new ProviderEvent
                    {
                        DataType = ProviderEventType.BanchoPacket,
                        Data = new BanchoPacket(new MatchTransferHost()),
                        ProviderType = ProviderType.ClientContext
                    });
                    
                    break;
                    
                case UserRequestTypes.SubscribeToMatchSetup:
                    await SubscriptionManager.SubscribeToMatchSetupUpdates(
                        (await Dependencies.Get<ILobbyProvider>().GetMatchSetupObservable(UserID))!);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request), request, null);
            }
        }
        
        public async Task LeaveChannel(string tag)
        {
            await SubscriptionManager.UnsubscribeFromChannel(tag);
            
            await SubscriptionManager.OnNext(this, new ProviderEvent
            {
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(new ChannelLeft() {ChannelName = tag}),
                ProviderType = ProviderType.ClientContext
            });
        }

        public async Task LeaveChannel(IChatChannel channel)
        {
            await SubscriptionManager.UnsubscribeFromChannel(channel);
            
            await SubscriptionManager.OnNext(this, new ProviderEvent
            {
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(new ChannelLeft() {ChannelName = channel.ChannelInfo.Tag}),
                ProviderType = ProviderType.ClientContext
            });
        }
        
        public async Task JoinChannel(IChatChannel channel)
        {
            await SubscriptionManager.SubscribeToChannel(channel);
            
            await SubscriptionManager.OnNext(this, new ProviderEvent
            {
                DataType = ProviderEventType.BanchoPacket,
                Data = new BanchoPacket(new ChannelJoined() {ChannelName = channel.ChannelInfo.Tag}),
                ProviderType = ProviderType.ClientContext
            });
        }

        public async Task InitialRegistration(UserInfo userInfo, Presence presence, Channel[] autojoinChannels)
        {
            var streamingProvider = Dependencies.Get<IStreamingProvider>();
            var chatProvider = Dependencies.Get<IChatProvider>();
            var userStateProvider = Dependencies.Get<IUserStateProvider>();
            var userRequestProvider = Dependencies.Get<IUserRequestProvider>();
            
            await SubscriptionManager.SubscribeToUserStateProvider(userStateProvider);

            await streamingProvider.RegisterStreamer(UserID);
            await SubscriptionManager.SubscribeToStreamerObservable((await streamingProvider.GetStreamerObserver(UserID))!);
            
            await SubscriptionManager.SubscribeToChat(chatProvider);
            await chatProvider.RegisterUser(Username);

            await using Database database = new Database();
            
            await userStateProvider.RegisterUserAsync(userInfo!.UserID,
                new UserData()
                {
                    Activity = new Activity {Action = Action.Idle},
                    Presence = presence,
                    Stats = await database.GetStatsWithRankAsync(userInfo.UserID, 0),
                    UserInfo = userInfo!
                });
            
            await userRequestProvider.RegisterUser(UserID);
            
            await SubscriptionManager.SubscribeToUserRequests(
                (await userRequestProvider.GetObservable(UserID))!);
            
            await SubscriptionManager.SubscribeToChannel(
                (await chatProvider.GetUserChannel(Username))!);

            foreach (var channel in autojoinChannels)
                await JoinChannel((await chatProvider.GetChannel(channel.Tag, Privileges))!);
        }
        

        // private readonly AsyncRwLockWrapper<GameSpectator> _gameSpectator;
        // private readonly AsyncRwLockWrapper<GameBroadcaster> _gameBroadcaster;

        private TaskCompletionSource _waitDisconnectionSource;

        public Task WaitDisconnection => _waitDisconnectionSource.Task;

        public void CompleteDisconnection()
        {
            _waitDisconnectionSource.SetResult();
        }
        
        public async ValueTask DisposeAsync()
        {
            var streamingProvider = Dependencies.Get<IStreamingProvider>();
            var chatProvider = Dependencies.Get<IChatProvider>();
            var userStateProvider = Dependencies.Get<IUserStateProvider>();
            var userRequestProvider = Dependencies.Get<IUserRequestProvider>();
            var lobbyProvider = Dependencies.Get<ILobbyProvider>();
            
            await chatProvider.UnregisterUser(Username);
            await userRequestProvider.UnregisterUser(UserID);
            await userStateProvider.UnregisterUserAsync(UserID);
            await streamingProvider.UnregisterStreamer(UserID);
            await lobbyProvider.TryLeaveMatch(UserID);
            await SubscriptionManager.DisposeAsync();
        }
    }
}