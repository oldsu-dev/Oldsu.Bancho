using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Fleck;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Types;
using Oldsu.Utils.Location;
using Action = Oldsu.Enums.Action;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class Server
    {
        private const string ServerVersion = "Alpha 0.1";

        private readonly AsyncLock _connectionLock;

        private readonly HubEventLoop _hubEventLoop;
        private readonly WebSocketServer _server;
        private readonly LoggingManager _loggingManager;
        
        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address, HubEventLoop hubEventLoop, LoggingManager loggingManager)
        {
            _server = new WebSocketServer(address, new HandlerSettings {Hybi13MaxMessageSize = 1024 * 1024}); // 1 MB Limit
            
            _loggingManager = loggingManager;
            _hubEventLoop = hubEventLoop;
            
            _connectionLock = new AsyncLock();
            
            // _userStateProvider = userDataProvider;
            // _streamingProvider = streamingProvider;
            // _lobbyProvider = lobbyProvider;
            // _userRequestProvider = userRequestProvider;
            // _chatProvider = chatProvider;
        }
        
        private static Version GetProtocol(string clientBuild) => clientBuild switch
        {
            "2100" => Version.B904,
            _ => Version.NotApplicable,
        };

        private async Task<(LoginResult, UserInfo?, Version, byte utcOffset, bool showCity, string[]? debugInfo)> Authenticate(string authString)
        {
            string[]? debugInfo = null;
            var authFields = authString.Split('\n').Select(s => s.Trim()).ToArray();

            if (authFields.Length != 3)
                return (LoginResult.TooOldVersion, null, Version.NotApplicable, 0, false, debugInfo);

            var (loginUsername, loginPassword, info) =
                (authFields[0], authFields[1], authFields[2]);

            debugInfo = new[] {loginUsername, info};
            var infoFields = info.Split("|");
            var version = GetProtocol(infoFields[0]);

            if (version == Version.NotApplicable)
                return (LoginResult.TooOldVersion, null, version, 0, false, debugInfo);

            await using var db = new Database();
            var user = await db.AuthenticateAsync(loginUsername, loginPassword);

            if (user == null)
                return (LoginResult.AuthenticationFailed, null, version, 0, false, debugInfo);

            if (user.Banned)
                return (LoginResult.Banned, null, version, 0, false, debugInfo);

            // user is found, user is not banned, client is not too old. Everything is fine.
            return (LoginResult.AuthenticationSuccessful, user, version,
                (byte)sbyte.Parse(infoFields[1]), infoFields[2] == "1", debugInfo);
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
                Privilege = user.Privileges,
                UtcOffset = (byte)(utcOffset + 24),
                Country = country,
                Longitude = locationX,
                Latitude = locationY
            };
        }

        private void HandlePacket(User user, ISharedPacketIn packet) =>
            _hubEventLoop.SendEvent(new HubEventPacket(user, packet));

        private async void HandleDisconnection(object? sender, EventArgs eventArgs)
        {
            var conn = (Connection) sender!;
        
            await _loggingManager.LogInfo<Server>("Client disconnected.", null, new
            {
                conn.IP,
                conn.Guid
            });
        }

        private async void HandleUserDisconnection(Connection connection, User user)
        {
            IDisposable handle = await _connectionLock.LockAsync();

            var disconnectEvent = new HubEventDisconnect(user);
            disconnectEvent.OnCompletion += () => { handle.Dispose(); };
            
            _hubEventLoop.SendEvent(disconnectEvent);
        }

        private async void HandleConnection(IWebSocketConnection webSocketConnection)
        {
            using (await _connectionLock.LockAsync())
            {
                var connection = new Connection(webSocketConnection);
                
                #region Logging

                _loggingManager.LogInfoSync<Server>("Client connected.", null, new
                {
                    connection.IP,
                    connection.Guid
                });

                #endregion

                connection.Disconnected += HandleDisconnection;
                connection.OnString += HandleLogin;
                connection.Ready += ConnectionReady;
                connection.Timedout += ConnectionTimedout;
            }
        }

        private void ConnectionTimedout(object? sender, EventArgs e)
        {
            Connection connection = (Connection) sender!;
            
            _loggingManager.LogInfoSync<Server>("Connection timed out.", dump: new
            {
                connection.IP,
                connection.Guid
            });
        }

        private void ConnectionReady(object? sender, EventArgs args)
        {
            Connection connection = (Connection) sender!;
            
            connection.SendPacket(SignaturePacket);
            connection.Ready -= ConnectionReady;
        }
        
        private async void HandleLogin(object? sender, string authString)
        {
            Connection connection = (Connection) sender!;
            
            connection.LockStateHolder.LockState();
            
            using (await _connectionLock.LockAsync())
            {
                if (connection.IsZombie)
                    return;

                #region Logging

                _loggingManager.LogInfoSync<Server>("Authenticating client.", null, new
                {
                    connection.IP
                });

                #endregion

                connection.OnString -= HandleLogin;

                (LoginResult, UserInfo?, Version, byte utcOffset, bool showCity, string[]? debugInfo) result;
                
                try
                {
                    result = await Authenticate(authString);
                }
                catch (Exception e)
                {
                    #region Logging

                    _loggingManager.LogCriticalSync<Server>("Error while authentication a client.", e, new
                    {
                        connection.IP,
                        connection.Guid
                    });

                    #endregion
                    
                    return;
                }

                var (loginResult, userInfo, version, utcOffset, showCity, _) = result;
                
                if (loginResult != LoginResult.AuthenticationSuccessful)
                {
                    #region Logging

                    _loggingManager.LogInfoSync<Server>("User authentication failed.", null, new
                    {
                        connection.IP,
                        connection.Guid,
                        result
                    });

                    #endregion
                    
                    connection.SendPacket(new Login {LoginStatus = (int) loginResult});
                    connection.LockStateHolder.UnlockState();
                    
                    await connection.Disconnect(false);

                    return;
                }

                #region Logging

                _loggingManager.LogInfoSync<Server>("User authenticated.", null, new
                {
                    result,
                    userInfo!.Username,
                    userInfo.UserID,
                    userInfo.Privileges,
                    Version = version,
                    ShowCity = showCity,
                    UtcOffset = utcOffset,
                    ConnectionGuid = connection.Guid
                });

                #endregion

                var presence = await GetPresenceAsync(userInfo, utcOffset, showCity, connection.IP);
                
                AuthenticateConnection(connection, version, userInfo, presence);
                
                connection.LockStateHolder.UnlockState();
            }
        }

        private static readonly CachedBanchoPacket SignaturePacket = new CachedBanchoPacket(new Signature
        {
            Version = ServerVersion,
            ServerName = "Oldsu!"
        });
        
        private void AuthenticateConnection(
            Connection connection, Version version, UserInfo userInfo, Presence presence)
        {
            var user = new User(userInfo, presence, new Activity {Action = Action.Idle}, null, connection);
            
            connection.Authenticate(version);
            connection.OnPacket += (_, packet) => HandlePacket(user, packet);
            connection.Disconnected += (connection,_) => HandleUserDisconnection((Connection)connection!, user);
            
            // Handshake
            
            connection.SendPacket(new Login {LoginStatus = (int) userInfo.UserID});
            connection.SendPacket(new BanchoPrivileges {Privileges = userInfo.Privileges});

            _hubEventLoop.SendEvent(new HubEventConnect(user));
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleConnection on client login.
        /// </summary>
        public async Task Run(CancellationToken ct = default)
        {
            await _loggingManager.LogInfo<Server>("Server is running.");

            try
            {
                _server.Start(HandleConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            await Task.WhenAny(_hubEventLoop.Run());
        }
    }
}
