using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Doron;
using Doron.Connections;
using Doron.Extensions;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Extensions;
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

            private readonly HubEventLoop _hubEventLoop;
        private readonly Doron.Server _server;
        private readonly LoggingManager _loggingManager;
        
        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(IPAddress address, int port, HubEventLoop hubEventLoop, LoggingManager loggingManager)
        {
            _server = new Doron.Server(address, port); // 1 MB Limit
            
            _loggingManager = loggingManager;
            _hubEventLoop = hubEventLoop;
            
            // _userStateProvider = userDataProvider;
            // _streamingProvider = streamingProvider;
            // _lobbyProvider = lobbyProvider;
            // _userRequestProvider = userRequestProvider;
            // _chatProvider = chatProvider;
        }
        
        private static Version GetProtocol(string clientBuild) => clientBuild switch
        {
            "2110" or "2100" or "2000" => Version.B904,
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

        private static readonly CachedBanchoPacket SignaturePacket = new CachedBanchoPacket(new Signature
        {
            Version = ServerVersion,
            ServerName = "Oldsu!"
        });

        private async Task HandlePacketStream(User user, BanchoConnection connection)
        {
            try
            {
                await foreach (var packet in connection.GetPacketEnumerable())
                    _hubEventLoop.SendEvent(new HubEventPacket(user, packet));
            }
            catch (TimeoutException)
            {
                _loggingManager.LogInfoSync<Server>("Connection timed out.", dump: new
                {
                    connection.IPAddress,
                    connection.WebSocketConnection.RawConnection.Guid
                });
            }
            catch (Exception exception)
            {
                _loggingManager.LogInfoSync<Server>("Exception was thrown when receiving packets.", exception, dump: new
                {
                    connection.IPAddress,
                    connection.WebSocketConnection.RawConnection.Guid
                });
            }

            _hubEventLoop.SendEvent(new HubEventDisconnect(user));

            await _loggingManager.LogInfo<Server>("Client disconnected.", null, new
            {
                connection.IPAddress,
                connection.WebSocketConnection.RawConnection.Guid
            });
        }

        private async Task AuthenticateConnection(WebSocketConnection connection)
        {
            using (connection)
            {
                BanchoConnection banchoConnection = new BanchoConnection(connection, Version.NotApplicable);
                await banchoConnection.SendPacketAsync(SignaturePacket);

                WebSocketConnection.WebSocketActionResult<WebSocketMessage> receiveResult =
                    await connection.ReceiveMessageAsync();

                if (receiveResult.Status != WebSocketConnection.WebSocketActionStatus.Ok)
                {
                    #region Logging

                    _loggingManager.LogInfoSync<Server>("not-Ok status when trying to receive authentication packet",
                        null, new { status = receiveResult.Status, IPAddress = connection.GetRealIPAddress(), exception = receiveResult.Exception });

                    #endregion
                    
                    return;
                }

                WebSocketMessage message = receiveResult.Result!;
                if (message is not WebSocketMessage.Text text)
                {
                    _loggingManager.LogInfoSync<Server>("authentication packet is not text.", dump: new {Type = message.GetType()});
                    
                    return;
                }

                (LoginResult, UserInfo?, Version, byte utcOffset, bool showCity, string[]? debugInfo) result;
                
                try
                {
                    result = await Authenticate(text.Data);
                }
                catch (Exception e)
                {
                    #region Logging

                    _loggingManager.LogCriticalSync<Server>("Error while authentication a client.", e, new
                    {
                        IPAddress = connection.GetRealIPAddress(),
                        connection.RawConnection.Guid
                    });

                    #endregion
                    
                    return;
                }

                var (loginResult, userInfo, version, utcOffset, showCity, _) = result;

                banchoConnection.Version = version;
                
                if (loginResult != LoginResult.AuthenticationSuccessful)
                {
                    #region Logging

                    _loggingManager.LogInfoSync<Server>("User authentication failed.", null, new
                    {
                        IPAddress = connection.GetRealIPAddress(),
                        connection.RawConnection.Guid
                    });

                    #endregion
                    
                    await banchoConnection.SendPacketAsync(new Login {LoginStatus = (int) loginResult});
                    await banchoConnection.DisconnectAsync();
                    
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
                    ConnectionGuid = banchoConnection.WebSocketConnection.RawConnection.Guid
                });

                #endregion
                
                var presence = await GetPresenceAsync(userInfo, utcOffset, showCity, 
                    banchoConnection.WebSocketConnection.RawConnection.RemoteIPAddress?.ToString() ?? string.Empty);

                var user = new User(userInfo, presence, new Activity {Action = Action.Idle}, null, banchoConnection);
                
                await banchoConnection.SendPacketAsync(new Login {LoginStatus = (int) userInfo.UserID});
                await banchoConnection.SendPacketAsync(new BanchoPrivileges {Privileges = userInfo.Privileges});

                _hubEventLoop.SendEvent(new HubEventConnect(user));

                await HandlePacketStream(user, banchoConnection);
            }
        }

        private async Task HandleConnections()
        {
            while (true)
            {
                WebSocketConnection connection = await _server.AcceptConnectionAsync();
                connection.MaxMessageLength = 1024 * 1024;
                _ = AuthenticateConnection(connection);
            }
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleConnection on client login.
        /// </summary>
        public async Task Run(CancellationToken ct = default)
        {
            await _loggingManager.LogInfo<Server>("Server is running.");
            
            await Task.WhenAny(HandleConnections(), _server.RunAsync(ct), _hubEventLoop.Run());
        }
    }
}
