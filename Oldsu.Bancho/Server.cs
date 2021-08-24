using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Handshakes;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Types;
using Oldsu.Utils.Location;
using Oldsu.Utils.Threading;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class Server
    {
        private const string ServerVersion = "Alpha 0.1";
            
        private readonly WebSocketServer _server;

        private AsyncRwLockWrapper<Dictionary<Guid, Connection>> _connections;

        private AsyncMutexWrapper<Dictionary<uint,
            (AuthenticatedConnection Connection, UserContext Context, TaskCompletionSource DisconnectionAwaiter)>> _authenticatedConnections;

        private SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

        private async Task PingWatchdog(CancellationToken ct = default)
        {
            for (;;)
            {
                if (ct.IsCancellationRequested)
                    return;

                var connections = await _connections.ReadAsync(connections => connections.Values.ToArray());
                
                foreach (var conn in connections.Where(c => c.PingTimeout))
                {
                    Console.WriteLine($"{conn.IP} timed out (${conn.GetType()}).");

                    if (conn.IsZombie)
                        conn.ForceDisconnect();
                    else
                        conn.Disconnect();
                }

                await Task.Delay(1000, ct);
            }
        }

        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address, DependencyManager dependencyManager, LoggingManager loggingManager)
        {
            _server = new WebSocketServer(address);
            _connections = new AsyncRwLockWrapper<Dictionary<Guid, Connection>>(new());

            _authenticatedConnections =
                new AsyncMutexWrapper<Dictionary<uint, (AuthenticatedConnection, UserContext, TaskCompletionSource)>>(new());

            _dependencyManager = dependencyManager;
            _loggingManager = loggingManager;
            
            // _userStateProvider = userDataProvider;
            // _streamingProvider = streamingProvider;
            // _lobbyProvider = lobbyProvider;
            // _userRequestProvider = userRequestProvider;
            // _chatProvider = chatProvider;
        }

        private DependencyManager _dependencyManager;
        private LoggingManager _loggingManager;

        private static Version GetProtocol(string clientBuild) => clientBuild switch
        {
            "2000" => Version.B904,
            _ => Version.NotApplicable,
        };

        private async Task<(LoginResult, UserInfo?, Version, byte utcOffset, bool showCity)> Authenticate(string authString)
        {
            try
            {
                var authFields = authString.Split().Select(s => s.Trim()).ToArray();

                if (authFields.Length != 3)
                    return (LoginResult.TooOldVersion, null, Version.NotApplicable, 0, false);

                var (loginUsername, loginPassword, info) =
                    (authFields[0], authFields[1], authFields[2]);

                var infoFields = info.Split("|");
                var version = GetProtocol(infoFields[0]);

                if (version == Version.NotApplicable)
                    return (LoginResult.TooOldVersion, null, version, 0, false);

                await using var db = new Database();
                var user = await db.AuthenticateAsync(loginUsername, loginPassword);

                if (user == null)
                    return (LoginResult.AuthenticationFailed, null, version, 0, false);

                if (user.Banned)
                    return (LoginResult.Banned, null, version, 0, false);

                // user is found, user is not banned, client is not too old. Everything is fine.
                return (LoginResult.AuthenticationSuccessful, user, version,
                    byte.Parse(infoFields[1]), infoFields[2] == "1");
            }
            catch
            {
                return (LoginResult.TooOldVersion, null, Version.NotApplicable, 0, false);
            }
        }

        private async Task<Presence> GetPresenceAsync(UserInfo user, byte utcOffset, bool showCity, string ip)
        {
            var (locationX, locationY, country) = showCity switch
            {
                true => await Geolocation.GetGeolocationAsync(ip),
                false => (0, 0, (byte)0)
            };

            return new Presence
            {
                Privilege = user!.Privileges,
                UtcOffset = (byte)(utcOffset + 24),
                Country = country,
                Longitude = locationX,
                Latitude = locationY
            };
        }

        private async void HandleDisconnection(object? sender, EventArgs eventArgs)
        {
            var conn = (Connection) sender!;
            
            await _connections.WriteAsync(connections => connections.Remove(conn.Guid));

            await _loggingManager.LogInfo<Server>("Client disconnected.", null, new
            {
                conn.IP
            });
        }

        private async Task DisconnectUser(uint userId)
        {
            UserContext context = default!;

            var (disposeTask, disconnectionAwaiter) = await _authenticatedConnections.LockAsync(connections =>
            {
                if (connections.TryGetValue(userId, out var authConnection))
                {
                    context = authConnection.Context;
                    return (authConnection.Context.DisposeAsync(), authConnection.DisconnectionAwaiter);
                }
                
                return (ValueTask.CompletedTask, (TaskCompletionSource?)null);
            });

            if (disposeTask == ValueTask.CompletedTask)
                return;

            try
            {
                await disposeTask;
                
                await _loggingManager.LogInfo<Server>("User disconnected.", null, new
                {
                    context.Username,
                    context.UserID,
                });
            }
            catch (Exception exception)
            {
                disconnectionAwaiter!.SetException(exception);
                throw;
            }

            await _authenticatedConnections.LockAsync(connections => connections.Remove(userId));
            
            disconnectionAwaiter!.SetResult();
        }

        private async void HandleUserDisconnection(uint userId) => await DisconnectUser(userId);

        private async void HandleConnection(IWebSocketConnection webSocketConnection)
        {
            var guid = Guid.NewGuid();
            var connection = new UnauthenticatedConnection(guid, webSocketConnection);
            
            await _connections.WriteAsync(connections => connections.Add(guid, connection));

            await _loggingManager.LogInfo<Server>("Client connected.", null, new
            {
                connection.IP,
            });
            
            connection.Disconnected += HandleDisconnection;
            connection.Login += HandleLogin;
        }

        private async void HandleLogin(object? sender, string authString)
        {
            UnauthenticatedConnection connection = (UnauthenticatedConnection) sender!;

            LockStateHolder lockStateHolder = connection.LockStateHolder;
            
            await _loggingManager.LogInfo<Server>("Client attempts to login.", null, new
            {
                connection.IP,
            });
            
            if (connection.IsZombie)
                return;

            lockStateHolder.LockState();
            await _connections.WriteAsync(connections => connections.Remove(connection.Guid));
            await _connectionSemaphore.WaitAsync();
            
            try
            {
                connection.Login -= HandleLogin;

                var (loginResult, userInfo, version, utcOffset, showCity) = await Authenticate(authString);

                if (loginResult != LoginResult.AuthenticationSuccessful)
                {
                    await connection.SendPacketAsync(new BanchoPacket(new Login {LoginStatus = (int) loginResult}));
                    connection.Disconnect();
                    
                    await _loggingManager.LogInfo<Server>("User authentication failed.", null, new
                    {
                        connection.IP
                    });
                    
                    return;
                }
                
                await _loggingManager.LogInfo<Server>("User authenticated.", null, new
                {
                    userInfo!.Username,
                    userInfo.UserID,
                    userInfo.Privileges,
                    Version = version,
                    ShowCity = showCity,
                    UtcOffset = utcOffset
                });
                
                var presence = await GetPresenceAsync(userInfo!, utcOffset, showCity, connection.IP);
            
                Task disconnectionTask = await _authenticatedConnections.LockAsync<Task>(connections =>
                {
                    if (!connections.TryGetValue(userInfo!.UserID, out var user)) 
                        return Task.CompletedTask;
                    
                    user.Connection.Disconnect();
                    return user.DisconnectionAwaiter.Task;
                });

                await disconnectionTask;

                var (upgradedConnection, context) = await UpgradeConnection(connection, version, userInfo!, presence);
                await _authenticatedConnections.LockAsync(connections => connections.Add(userInfo!.UserID, 
                    (upgradedConnection, context, new TaskCompletionSource())));
            }
            finally
            {
                _connectionSemaphore.Release();
                lockStateHolder.UnlockState();
            }
        }

        private static readonly BanchoPacket _signaturePacket = new BanchoPacket(new Signature
        {
            Version = ServerVersion,
            ServerName = "Oldsu!"
        });

        private async Task<(AuthenticatedConnection, UserContext)> UpgradeConnection(
            UnauthenticatedConnection connection, Version version, UserInfo userInfo, Presence presence)
        {
            using var connectionsLock = await _connections.AcquireWriteLockGuard();
            
            var userContext = new UserContext(userInfo.UserID, userInfo.Username, userInfo.Privileges, 
                _dependencyManager, _loggingManager);

            var upgradedConnection = await connection.Upgrade(version);

            connectionsLock.Value.Add(upgradedConnection.Guid, upgradedConnection);
            
            var handler = new ConnectionEventHandler(_loggingManager, userContext, upgradedConnection);

            // Bind connection events
            userContext.SubscriptionManager.PacketInbound += (_, packet) => handler.PacketInbound(packet);
            userContext.SubscriptionManager.EventInbound += (_, packet) => handler.UserRequestInbound((UserRequest)packet.Data!);
            
            upgradedConnection.PacketReceived += (_, packet) => handler.ProcessPacket(packet);

            upgradedConnection.Disconnected += HandleDisconnection;
            
            HandleUserDisconnection(userInfo.UserID);
            
            await upgradedConnection.SendPacketAsync(_signaturePacket);

            var chatProvider = _dependencyManager.Get<IChatProvider>();
            var userStateProvider = _dependencyManager.Get<IUserStateProvider>();
            
            var autojoinChannels = await chatProvider.GetAutojoinChannelInfo(userInfo.Privileges);
            var availableChannels = await chatProvider.GetAvailableChannelInfo(userInfo.Privileges);

            List<Friendship> friendships;
            await using (var database = new Database()) 
            {
                friendships = await database.Friends
                    .Where(friendship => friendship.UserID == userInfo.UserID)
                    .ToListAsync();
            }

            await upgradedConnection.SendPacketAsync(
                new BanchoPacket(new Login() {LoginStatus = (int) userInfo.UserID}));
            
            await upgradedConnection.SendHandshake(
                new CommonHandshake(
                    await userStateProvider.GetAllUsersAsync(),
                    presence.Privilege,
                    autojoinChannels,
                    availableChannels,
                    friendships
                ));
            
            await userContext.InitialRegistration(userInfo, presence, autojoinChannels);

            return (upgradedConnection, userContext);
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleLoginAsync on client login.
        /// </summary>
        public async Task Run(CancellationToken ct = default)
        {
            await _loggingManager.LogInfo<Server>("Server is running.");
            
            await Task.Factory.StartNew(() => PingWatchdog(ct), ct);
            
            try
            {
                _server.Start(HandleConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(-1, ct);
        }
    }
}