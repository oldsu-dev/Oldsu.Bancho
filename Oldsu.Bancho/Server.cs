using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using MaxMind.Db;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Handshakes;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;
using Oldsu.Utils.Threading;
using Action = Oldsu.Enums.Action;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class Server
    {
        private const string ServerVersion = "Alpha 0.1";
            
        private readonly WebSocketServer _server;

        private AsyncRwLockWrapper<Dictionary<Guid, Connection>> _connections;

        private AsyncMutexWrapper<Dictionary<uint,
            (AuthenticatedConnection Connection, UserContext Context)>> _authenticatedConnections;

        private async Task PingWatchdog(CancellationToken ct = default)
        {
            for (;;)
            {
                if (ct.IsCancellationRequested)
                    return;

                await _connections.ReadAsync(connections =>
                {
                    foreach (var conn in connections.Values.Where(c => c.PingTimeout))
                    {
                        Console.WriteLine($"{conn.ConnectionInfo.Host} timed out (${conn.GetType()}).");

                        if (conn.IsZombie)
                            conn.ForceDisconnect();
                        else
                            conn.Disconnect();
                    }
                });

                await Task.Delay(1000, ct);
            }
        }

        /// <summary>
        ///     Initializes the websocket class
        /// </summary>
        /// <param name="address">Address to bind server to. Example: ws://127.0.0.1:3000</param>
        public Server(string address, 
            IUserStateProvider userDataProvider, 
            IStreamingProvider streamingProvider, 
            ILobbyProvider lobbyProvider,
            IUserRequestProvider userRequestProvider,
            IChatProvider chatProvider)
        {
            _server = new WebSocketServer(address);
            _connections = new AsyncRwLockWrapper<Dictionary<Guid, Connection>>(new());

            _authenticatedConnections =
                new AsyncMutexWrapper<Dictionary<uint, 
                    (AuthenticatedConnection Connection, UserContext Context)>>(new());

            _userStateProvider = userDataProvider;
            _streamingProvider = streamingProvider;
            _lobbyProvider = lobbyProvider;
            _userRequestProvider = userRequestProvider;
            _chatProvider = chatProvider;
        }

        private IUserStateProvider _userStateProvider;
        private IStreamingProvider _streamingProvider;
        private ILobbyProvider _lobbyProvider;
        private IUserRequestProvider _userRequestProvider;
        private IChatProvider _chatProvider;

        private static Version GetProtocol(string clientBuild) => clientBuild switch
        {
            "2000" => Version.B904,
            _ => Version.NotApplicable,
        };

        private static readonly ConcurrentDictionary<string, GeoLoc> GeoLocCache = new();
        private static readonly Reader IpLookupDatabase = new("GeoLite2-City.mmdb", FileAccessMode.MemoryMapped);

        private static async Task<(float, float)> GetGeolocationAsync(string ip)
        {
            if (ip == "127.0.0.1")
                return (0, 0);
            
            var data = IpLookupDatabase.Find<Dictionary<string, object>>(IPAddress.Parse(ip));
            
            if (data != null)
            {
                var location = (Dictionary<string, object>)data["location"];
                return ((float)location["latitude"], (float)location["longitude"]);
            }

            if (GeoLocCache.TryGetValue(ip, out var geoLoc))
                return (geoLoc.Lat, geoLoc.Lon);

            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetAsync($"http://ip-api.com/json/{ip}");
                geoLoc = JsonConvert.DeserializeObject<GeoLoc>(await json.Content.ReadAsStringAsync());
            }

            GeoLocCache.TryAdd(ip, geoLoc!);
            return (geoLoc!.Lat, geoLoc!.Lon);
        }
        
        private async Task<(LoginResult, UserInfo?, Version)> Authenticate(string authString)
        {
            var authFields = authString.Split().Select(s => s.Trim()).ToArray();
            
            if (authFields.Length != 3)
                return (LoginResult.TooOldVersion, null, Version.NotApplicable);
                
            var (loginUsername, loginPassword, info) =
                (authFields[0], authFields[1], authFields[2]);
        
            var version = GetProtocol(info.Split("|")[0]);

            if (version == Version.NotApplicable)
                return (LoginResult.TooOldVersion, null, version);

            await using var db = new Database();
            var user = await db.AuthenticateAsync(loginUsername, loginPassword);

            if (user == null)
                return (LoginResult.AuthenticationFailed, null, version);

            if (user.Banned)
                return (LoginResult.Banned, null, version);

            // user is found, user is not banned, client is not too old. Everything is fine.
            return (LoginResult.AuthenticationSuccessful, user, version);
        }

        private async Task<Presence> GetPresenceAsync(UserInfo user, string ip)
        {
            var (locationX, locationY) = await GetGeolocationAsync(ip);

            return new Presence
            {
                Privilege = user!.Privileges,
                UtcOffset = 0,
                Country = user!.Country,
                Longitude = locationX,
                Latitude = locationY
            };
        }

        private async void HandleDisconnection(object? sender, EventArgs eventArgs)
        {
            var conn = (Connection) sender!;
            
            Debug.WriteLine($"{conn.ConnectionInfo.Host} disconnected. (${conn.GetType()}).");
            await _connections.WriteAsync(connections => connections.Remove(conn.Guid));

            await conn.DisposeAsync();
        }
        
        private async void HandleUserDisconnection(uint userId)
        {
            using var authenticatedConnectionsLock = await _authenticatedConnections.AcquireLockGuard();
            
            authenticatedConnectionsLock.Value.Remove(userId);
        }

        private async void HandleConnection(IWebSocketConnection webSocketConnection)
        {
            var guid = Guid.NewGuid();
            var connection = new UnauthenticatedConnection(guid, webSocketConnection);

            await _connections.WriteAsync(connections => connections.Add(guid, connection));

            connection.Disconnected += HandleDisconnection;
            connection.Login += HandleLogin;
        }

        private async void HandleLogin(object? sender, string authString)
        {
            UnauthenticatedConnection connection = (UnauthenticatedConnection) sender!;
            connection.Login -= HandleLogin;

            var (loginResult, userInfo, version) = await Authenticate(authString);

            if (loginResult != LoginResult.AuthenticationSuccessful)
            {
                await connection.SendPacketAsync(new BanchoPacket(new Login {LoginStatus = (int) loginResult}));
                connection.Disconnect();
                return;
            }

            using (var authenticatedConnectionsLock = await _authenticatedConnections.AcquireLockGuard())
                if (authenticatedConnectionsLock.Value.TryGetValue(userInfo!.UserID, out var user))
                {
                    user.Connection.Disconnect();
                    await user.Context.WaitDisconnection;
                }

            await UpgradeConnection(connection, version, userInfo!);
        }

        private static readonly BanchoPacket _signaturePacket = new BanchoPacket(new Signature
        {
            Version = ServerVersion,
            ServerName = "Oldsu!"
        });
        
        private async Task UpgradeConnection(UnauthenticatedConnection connection, Version version, UserInfo userInfo)
        {
            using var connectionsLock = await _connections.AcquireWriteLockGuard();
            using var authenticatedConnectionsLock = await _authenticatedConnections.AcquireLockGuard();

            var userContext = new UserContext(userInfo.UserID, userInfo.Username, userInfo.Privileges,
                _userStateProvider, _streamingProvider, _lobbyProvider, _userRequestProvider, _chatProvider);
            
            var presence = await GetPresenceAsync(userInfo!,
                connection.ConnectionInfo.Headers.TryGetValue("X-Forwaded-For", out var ip)
                    ? ip
                    : connection.ConnectionInfo.ClientIpAddress);
            
            var upgradedConnection = await connection.Upgrade(version);

            var handler = new ConnectionEventHandler(userContext, upgradedConnection);

            userContext.SubscriptionManager.PacketInbound += (_, packet) => handler.PacketInbound(packet);
            userContext.SubscriptionManager.EventInbound += (_, packet) => 
                handler.UserRequestInbound((UserRequestTypes)packet.Data!);
            
            upgradedConnection.PacketReceived += (_, packet) => handler.ProcessPacket(packet);
            upgradedConnection.Disconnected += (_, _) => handler.DisposeUserContext();

            // Update connection object
            connectionsLock.Value[upgradedConnection.Guid] = upgradedConnection;

            authenticatedConnectionsLock.Value.Remove(userInfo.UserID);
            authenticatedConnectionsLock.Value.Add(userInfo.UserID, (upgradedConnection, userContext));
            
            upgradedConnection.Disconnected += HandleDisconnection;
            upgradedConnection.Disconnected += (_, _) => HandleUserDisconnection(userInfo.UserID);

            await upgradedConnection.SendPacketAsync(_signaturePacket);

            var autojoinChannels = await _chatProvider.GetAutojoinChannelInfo(userInfo.Privileges);
            var availableChannels = await _chatProvider.GetAvailableChannelInfo(userInfo.Privileges);
            
            await upgradedConnection.SendHandshake(
                new CommonHandshake(
                    await _userStateProvider.GetAllUsersAsync(),
                    userInfo.UserID,
                    presence.Privilege,
                    autojoinChannels,
                    availableChannels));
            
            await userContext.InitialRegistration(userInfo, presence, autojoinChannels);
        }

        /// <summary>
        ///     Starts a websocket server and listening to incoming traffic.
        ///     Also calls HandleLoginAsync on client login.
        /// </summary>
        public async Task Run(CancellationToken ct = default)
        {
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