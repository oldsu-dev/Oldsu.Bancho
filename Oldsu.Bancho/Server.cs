﻿using System;
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
using Channel = System.Threading.Channels.Channel;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class Server
    {
        private const string ServerVersion = "Alpha 0.1";

        private readonly Dictionary<Guid, Connection> _connections;
        private AsyncLock _connectionLock;

        private readonly HubEventLoop _hubEventLoop;
        private readonly WebSocketServer _server;
        private readonly LoggingManager _loggingManager;
        
        private async Task PingWatchdog(CancellationToken ct = default)
        {

            for (;;)
            {
                if (ct.IsCancellationRequested)
                    return;

                using (await _connectionLock.LockAsync())
                {
                    foreach (var conn in _connections.Values.Where(c => c.IsTimedout))
                    {
                        if (conn.IsZombie)
                            _connections.Remove(conn.Guid);
                        else
                        {
                            _loggingManager.LogInfoSync<Server>("Client timed out.", null, new
                            {
                                conn.IP
                            });

                            conn.ForceDisconnect();
                        }
                    }
                }

                await Task.Delay(1000, ct);
            }
        }
        
        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address, HubEventLoop hubEventLoop, LoggingManager loggingManager)
        {
            _server = new WebSocketServer(address, new HandlerSettings {Hybi13MaxMessageSize = 1024 * 1024}); // 1 MB Limit
            
            _loggingManager = loggingManager;
            _hubEventLoop = hubEventLoop;
            
            _connections = new Dictionary<Guid, Connection>();
            _connectionLock = new AsyncLock();
            
            // _userStateProvider = userDataProvider;
            // _streamingProvider = streamingProvider;
            // _lobbyProvider = lobbyProvider;
            // _userRequestProvider = userRequestProvider;
            // _chatProvider = chatProvider;
        }
        
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
                    (byte)sbyte.Parse(infoFields[1]), infoFields[2] == "1");
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
            
            using (await _connectionLock.LockAsync())
            {
                _connections.Remove(conn.Guid);

                await _loggingManager.LogInfo<Server>("Client disconnected.", null, new
                {
                    conn.IP
                });
            }
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
                var guid = Guid.NewGuid();

                var connection = new Connection(guid, webSocketConnection);
                _connections.Add(guid, connection);
                
                #region Logging

                await _loggingManager.LogInfo<Server>("Client connected.", null, new
                {
                    connection.IP
                });

                #endregion

                connection.Disconnected += HandleDisconnection;
                connection.OnString += HandleLogin;
                connection.Ready += ConnectionReady;
            }
        }

        private async void ConnectionReady(object? sender, EventArgs args)
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

                await _loggingManager.LogInfo<Server>("Authenticating client.", null, new
                {
                    connection.IP
                });

                #endregion

                connection.OnString -= HandleLogin;

                var (loginResult, userInfo, version, utcOffset, showCity) = await Authenticate(authString);

                if (loginResult != LoginResult.AuthenticationSuccessful)
                {
                    #region Logging

                    await _loggingManager.LogInfo<Server>("User authentication failed.", null, new
                    {
                        connection.IP
                    });

                    #endregion
                    
                    connection.SendPacket(new Login {LoginStatus = (int) loginResult});
                    connection.LockStateHolder.UnlockState();
                    
                    await connection.Disconnect(false);

                    return;
                }

                #region Logging

                await _loggingManager.LogInfo<Server>("User authenticated.", null, new
                {
                    userInfo!.Username,
                    userInfo.UserID,
                    userInfo.Privileges,
                    Version = version,
                    ShowCity = showCity,
                    UtcOffset = utcOffset
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
            
            await Task.WhenAny(_hubEventLoop.Run(), PingWatchdog(ct));
        }
    }
}