﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fleck;
using Newtonsoft.Json;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared;
using Oldsu.Types;
using osuserver2012.Enums;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{ 
    /// <summary>
    ///     Class for each user in oldsu's bancho
    /// </summary>
    public class Client
    {
        /// <summary>
        ///     Key-Value dictionary of all clients.
        ///     TODO: Implement a multiple key-value dictionary.
        /// </summary>
        public static ConcurrentDictionary<int, Client> Clients = new();

        private IWebSocketConnection _webSocketConnection;
        
        public User User;
        public Stats Stats;
        public Presence Presence;

        public Version Version;

        public Client(IWebSocketConnection webSocketConnection)
        {
            _webSocketConnection = webSocketConnection;
        }

        /// <summary>
        ///     Sends packet to client.
        /// </summary>
        /// <param name="sharedPacket"> Packet meant to be sent. </param>
        private async Task SendPacket(ISharedPacket sharedPacket) {
            object packet = null;  

            switch (this.Version) {
                case Version.B394A:
                    packet = ((Into<IB394APacketOut>)sharedPacket).Into();
                    break;
            } 

            byte[] data = BanchoSerializer.Serialize(packet);

            await _webSocketConnection.Send(data);
        }
        
        /// <summary>
        ///     Handles incoming login requests accordingly.
        /// </summary>
        /// <param name="authenticationString"> Authentication string that osu! sends on login. </param>
        public async Task HandleLoginAsync(string authenticationString)
        {
            var (loginStatus, user, version) = await AuthenticateAsync(authenticationString.Replace("\r", "").Split("\n"));
            
            User = user;
            Version = version;

            switch (loginStatus)
            {
                case LoginResult.AuthenticationSuccessful:
                    var db = new Database();

                    Stats = await db.Stats.FindAsync(User.UserID);
                    
                    await SendPacket(new Login { LoginStatus = (int)loginStatus, Privilege = (byte)User.Privileges });

                    break;
                
                default:
                    await SendPacket(new Login { LoginStatus = (int)loginStatus, Privilege = 0 });
                    
                    break;
            }
        }

        /// <summary>
        ///     Returns the authentication result of the user.
        /// </summary>
        /// <param name="authenticationString"> Authentication string seperated by \n </param>
        /// <returns> Result of the authentication. the User and Version variables get returned, if the authentication was successful </returns>
        private static async Task<(LoginResult, User, Version)> AuthenticateAsync(IReadOnlyList<string> authenticationString)
        {
            var (loginUsername, loginPassword, info) =
                (authenticationString[0], authenticationString[1], authenticationString[2]);

            var version = GetProtocol(info.Split("|")[0]);
            
            if (version == Version.NotApplicable)
                return (LoginResult.TooOldVersion, null, version);
            
            await using var db = new Database();

            var user = await db.Authenticate(loginUsername, loginPassword);

            if (user == null)
                return (LoginResult.AuthenticationFailed, null, version);
            
            if (user.Banned == true)
                return (LoginResult.Banned, null, version);

            // user is found, user is not banned, client is not too old. Everything is fine.
            return (LoginResult.AuthenticationSuccessful, user, version);
        }

        private static Version GetProtocol(string clientBuild) => clientBuild switch {
            "1520" => Version.B394A,
            _ => Version.NotApplicable,
        };
        
        private static async Task<(float, float)> RetrieveGeoLocationAsync(string ip)
        {
            var json = await new HttpClient().GetAsync($"http://ip-api.com/json/{ip}");

            var geoLoc = JsonConvert.DeserializeObject<GeoLocSerialization>(await json.Content.ReadAsStringAsync());

            return (geoLoc.Lat, geoLoc.Lon);
        }
    }
}