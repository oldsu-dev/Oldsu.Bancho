using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Fleck;
using MaxMind.Db;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Oldsu.Bancho.Objects;
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
        public static ConcurrentDictionary<uint, Client> Clients = new();

        private IWebSocketConnection? _webSocketConnection;
        
        public User? User;
        public Stats? Stats;
        public UserActivity? Activity;
        public Presence? Presence;

        public Version Version;

        public void BindWebSocket(IWebSocketConnection webSocketConnection)
        {
            _webSocketConnection = webSocketConnection;

            _webSocketConnection.OnMessage = async message => await this.HandleLoginAsync(message);
            _webSocketConnection.OnBinary = async data => await this.HandleDataAsync(data);
        }

        /// <summary>
        ///     Sends packet to client.
        /// </summary>
        /// <param name="sharedPacket"> Packet meant to be sent. </param>
        private async Task SendPacket(BanchoPacket packet)
        {
            try
            {
                var x = packet.GetDataByVersion(this.Version);
                await _webSocketConnection!.Send(x);
            }
            catch (ConnectionNotAvailableException exception)
            {
                Disconnect();
            }
        }

        public async Task HandleDataAsync(byte[] data)
        {
            if (this.User == null)
            {
                Disconnect();
                return;
            }

            var obj = BanchoSerializer.Deserialize(data, this.Version);

            if (obj == null)
            {
                return;
            }
            
            ISharedPacket packet = ((Into<ISharedPacket>)obj).Into();

            Console.WriteLine(packet);
            
            switch (packet)
            {
                case UserActivity activity:
                    this.Activity = activity;
                    
                    await SendPacket(new BanchoPacket(
                        new StatusUpdate { Client = this })
                    );
                    break;
                
                case UserStatsRequest _:
                    await SendPacket(new BanchoPacket(
                        new StatusUpdate { Client = this })
                    );
                    break;
            }
        }
        
        /// <summary>
        ///     Handles incoming login requests accordingly.
        /// </summary>
        /// <param name="authenticationString"> Authentication string that osu! sends on login. </param>
        public async Task HandleLoginAsync(string authenticationString)
        {
            var (loginStatus, user, version) = await AuthenticateAsync(authenticationString.Replace("\r", "").Split("\n"));
            //var (x, y) = await GetGeolocationAsync(_webSocketConnection.ConnectionInfo.ClientIpAddress);

            switch (loginStatus)
            {
                case LoginResult.AuthenticationSuccessful:
                    var db = new Database();

                    User = user;
                    Version = version;
                    Stats = await db.Stats
                                        .Where(s => s.UserID == User!.UserID)
                                        .FirstAsync();

                    Activity = new UserActivity();
                    
                    Clients.TryAdd(User!.UserID, this);

                    await SendPacket(new BanchoPacket(
                        new Login { LoginStatus = (int)user!.UserID, Privilege = (byte)User.Privileges })
                    );

                    await SendPacket(new BanchoPacket(
                        new StatusUpdate { Client = this })
                    );
                    
                    foreach (var c in Clients.Values)
                    {
                        await SendPacket(new BanchoPacket(
                            new SetPresence { Client = this })
                        );   
                    }

                    break;
                
                default:
                    await SendPacket(new BanchoPacket(
                        new Login { LoginStatus = (int)loginStatus, Privilege = 0 })
                    );
                    
                    break;
            }
        }

        /// <summary>
        ///     Disconnects client from the server.
        /// </summary>
        public void Disconnect()
        {
            _webSocketConnection!.Close();
            
            if (User != null) 
                Clients.TryRemove(User.UserID, out _);
        }

        /// <summary>
        ///     Returns the authentication result of the user.
        /// </summary>
        /// <param name="authenticationString"> Authentication string seperated by \n </param>
        /// <returns> Result of the authentication. the User and Version variables get returned, if the authentication was successful </returns>
        private static async Task<(LoginResult, User?, Version)> AuthenticateAsync(IReadOnlyList<string> authenticationString)
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
        
        private static ConcurrentDictionary<string, GeoLoc> _geoLocCache = new();
        private static Reader _ipLookupDatabase = new("GeoLite2-City.mmdb", FileAccessMode.Memory);
        private static HttpClient _httpClient = new();
        
        private static async Task<(float, float)> GetGeolocationAsync(string ip)
        {
            var data = _ipLookupDatabase.Find<Dictionary<string, object>>(IPAddress.Parse(ip));
            
            if (data != null)
            {
                var location = (Dictionary<string, object>)data["location"];
                return ((float)location["latitude"], (float)location["longitude"]);
            }

            if (ip == "127.0.0.1")
                return (0, 0);

            if (_geoLocCache.TryGetValue(ip, out var geoLoc))
                return (geoLoc.Lat, geoLoc.Lon);

            var json = await _httpClient.GetAsync($"http://ip-api.com/json/{ip}");
            geoLoc = JsonConvert.DeserializeObject<GeoLoc>(await json.Content.ReadAsStringAsync());

            _geoLocCache.TryAdd(ip, geoLoc);

            return (geoLoc.Lat, geoLoc.Lon);
        }
    }
}