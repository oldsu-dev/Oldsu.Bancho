﻿using System;
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
            
            Debug.WriteLine($"{conn.ConnectionInfo.ClientIpAddress} disconnected. (${conn.GetType()}).");
            await _connections.WriteAsync(connections => connections.Remove(conn.Guid));
        }
        
        private async void HandleUserDisconnection(Guid guid, uint userId)
        {
            using var authenticatedConnectionsLock = await _authenticatedConnections.AcquireLockGuard();

            if (authenticatedConnectionsLock.Value.TryGetValue(userId, out var authConnection) &&
                authConnection.Connection.Guid == guid)
            {
                await authConnection.Context.DisposeAsync();
                authenticatedConnectionsLock.Value.Remove(userId);
            }
        }

        private async void HandleConnection(IWebSocketConnection webSocketConnection)
        {
            var guid = Guid.NewGuid();
            var connection = new UnauthenticatedConnection(guid, webSocketConnection);
            
            Console.WriteLine($"{connection.IP} trying to connect.");

            await _connections.WriteAsync(connections => connections.Add(guid, connection));

            connection.Disconnected += HandleDisconnection;
            connection.Login += HandleLogin;
        }

        private async void HandleLogin(object? sender, string authString)
        {
            UnauthenticatedConnection connection = (UnauthenticatedConnection) sender!;

            connection.Login -= HandleLogin;

            var (loginResult, userInfo, version, utcOffset, showCity) = await Authenticate(authString);

            if (loginResult != LoginResult.AuthenticationSuccessful)
            {
                await connection.SendPacketAsync(new BanchoPacket(new Login {LoginStatus = (int) loginResult}));
                connection.Disconnect();
                return;
            }
            
            var presence = await GetPresenceAsync(userInfo!, utcOffset, showCity, connection.IP);

            using var authenticatedConnectionsLock = await _authenticatedConnections.AcquireLockGuard();
            if (authenticatedConnectionsLock.Value.TryGetValue(userInfo!.UserID, out var user))
            {
                user.Connection.Disconnect();
                await user.Context.WaitDisconnection;
                authenticatedConnectionsLock.Value.Remove(userInfo.UserID);
            }

            var result = await UpgradeConnection(connection, version, userInfo!, presence);
            
            authenticatedConnectionsLock.Value.Remove(userInfo.UserID);
            authenticatedConnectionsLock.Value.Add(userInfo.UserID, result);
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
                _userStateProvider, _streamingProvider, _lobbyProvider, _userRequestProvider, _chatProvider);

            var upgradedConnection = await connection.Upgrade(version);

            connectionsLock.Value[upgradedConnection.Guid] = upgradedConnection;
            
            var handler = new ConnectionEventHandler(userContext, upgradedConnection);

            // Bind connection events
            userContext.SubscriptionManager.PacketInbound += (_, packet) => handler.PacketInbound(packet);
            userContext.SubscriptionManager.EventInbound += (_, packet) => handler.UserRequestInbound((UserRequestTypes)packet.Data!);
            
            upgradedConnection.PacketReceived += (_, packet) => handler.ProcessPacket(packet);
            
            upgradedConnection.Disconnected += HandleDisconnection;
            upgradedConnection.Disconnected += (_, _) => HandleUserDisconnection(upgradedConnection.Guid, userInfo.UserID);
            
            await upgradedConnection.SendPacketAsync(_signaturePacket);

            var autojoinChannels = await _chatProvider.GetAutojoinChannelInfo(userInfo.Privileges);
            var availableChannels = await _chatProvider.GetAvailableChannelInfo(userInfo.Privileges);
            
            await userContext.InitialRegistration(userInfo, presence, autojoinChannels);

            List<Friendship> friendships;
            await using (var database = new Database()) 
            {
                friendships = await database.Friends
                    .Where(friendship => friendship.UserID == userInfo.UserID)
                    .ToListAsync();
            }

            await upgradedConnection.SendHandshake(
                new CommonHandshake(
                    await _userStateProvider.GetAllUsersAsync(),
                    userInfo.UserID,
                    presence.Privilege,
                    autojoinChannels,
                    availableChannels,
                    friendships
                ));

            return (upgradedConnection, userContext);
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