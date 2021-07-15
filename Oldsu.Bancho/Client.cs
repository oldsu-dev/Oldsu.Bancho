using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
using Oldsu.Bancho.Packet.Out.B904;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Types;
using osuserver2012.Enums;
using SendMessage = Oldsu.Bancho.Packet.Shared.Out.SendMessage;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{ 
    public class ClientInfo
    {
        public User User;
        public Stats? Stats;
        public UserActivity Activity;
        public Presence Presence;    
    }
    
    /// <summary>
    ///     Class for each user in oldsu's bancho
    /// </summary>
    public class Client
    {
        public ClientInfo? ClientInfo { get; private set; }

        /// <summary>
        ///     Key-Value dictionary of all clients.
        ///     TODO: Implement a multiple key-value dictionary.
        /// </summary>

        private Guid _uuid;
        private IWebSocketConnection? _webSocketConnection;

        public const int AuthTimeoutPeriod = 10_000;
        public const int PingTimeoutPeriod = 35_000;

        public DateTime PingTimeoutWindow { get; private set; } = DateTime.MinValue;
        
        public Version Version { get; private set; }= Version.NotApplicable;

        public void ResetPing(int nextPeriod)
        { 
            PingTimeoutWindow = DateTime.Now + new TimeSpan(0,0,0,0, nextPeriod);
        }

        public void BindWebSocket(IWebSocketConnection webSocketConnection)
        {
            _webSocketConnection = webSocketConnection;

            _webSocketConnection.OnMessage += HandleLoginAsync;
            _webSocketConnection.OnBinary += HandleDataAsync;
            _webSocketConnection.OnClose += HandleClose;

            _uuid = new Guid();
            Server.Clients.TryAdd(_uuid, this);

            ResetPing(AuthTimeoutPeriod);
        }

        /// <summary>
        ///     Sends packet to client.
        /// </summary>
        /// <param name="packet"> Packet meant to be sent. </param>
        public async Task SendPacket(BanchoPacket packet)
        {
            if (!_webSocketConnection!.IsAvailable)
                return;
            
            try
            {
                var data = packet.GetDataByVersion(this.Version);
                
                if (data.Length == 0)
                    return;

                await _webSocketConnection!.Send(data);
            }
            catch (ConnectionNotAvailableException exception)
            {
                Debug.WriteLine(exception);
                //Disconnect();
            }
        }

        private async void HandleDataAsync(byte[] data)
        {
            if (ClientInfo == null)
            {
                Disconnect();
                return;
            }
            
            ResetPing(PingTimeoutPeriod);
            
            var obj = BanchoSerializer.Deserialize(data, this.Version);

            if (obj == null)
            {
                return;
            }
            
            ISharedPacketIn packet = ((Into<ISharedPacketIn>)obj).Into();
                
            await packet.Handle(this);
        }

        /// <summary>
        ///     Handles incoming login requests accordingly.
        /// </summary>
        /// <param name="authenticationString"> Authentication string that osu! sends on login. </param>
        public async void HandleLoginAsync(string authenticationString)
        {
            var (loginStatus, user, version) = await AuthenticateAsync(
                authenticationString.Replace("\r", "").Split("\n"));
            
            ResetPing(PingTimeoutPeriod);
            
            var (x, y) = await GetGeolocationAsync(_webSocketConnection!.ConnectionInfo.ClientIpAddress);

            switch (loginStatus)
            {
                case LoginResult.AuthenticationSuccessful:
                    var db = new Database();

                    Console.WriteLine("{0} connected.", user!.Username);
                    
                    ClientInfo = new ClientInfo
                    {
                        User = user,
                        Activity = new UserActivity(),
                        Presence = new Presence
                        {
                            Privilege = user.Privileges,
                            UtcOffset = 0,
                            Country = 0,
                            Longitude = x,
                            Latitude = y
                        },
                        Stats = await db.Stats
                            .Where(s => s.UserID == user.UserID)
                            .FirstAsync()
                    };
                    
                    Version = version;
                    
                    Server.AuthenticatedClients.TryAdd(user.UserID, user.Username, this);

                    await SendPacket(new BanchoPacket(
                        new Login { LoginStatus = (int)user.UserID }
                    ));

                    foreach (var c in Server.AuthenticatedClients.Values)
                    {
                        await SendPacket(new BanchoPacket(
                            new SetPresence { ClientInfo = c.ClientInfo! })
                        );   
                    }

                    Server.BroadcastPacket(new BanchoPacket( 
                            new SetPresence { ClientInfo = ClientInfo })
                    );

                    await SendPacket(new BanchoPacket(
                        new JoinChannel { ChannelName = "#osu" }
                    ));
                    
                    await SendPacket(new BanchoPacket(
                        new SendMessage
                        {
                            Sender = "ouigfdbnougfdbofd",
                            Contents = "HELLO from the server.",
                            Target = "#osu"
                        }
                    ));
                    
                    break;
                
                default:
                    await SendPacket(new BanchoPacket(
                        new Login { LoginStatus = (int)loginStatus })
                    );
                    
                    break;
            }
        }

        public void HandleClose()
        {
            _webSocketConnection!.OnMessage -= HandleLoginAsync;
            _webSocketConnection!.OnBinary -= HandleDataAsync;
            _webSocketConnection!.OnClose -= HandleClose;

            if (ClientInfo != null)
            {
                Server.BroadcastPacket(new BanchoPacket(
                        new UserQuit { UserID = (int)ClientInfo?.User.UserID! })
                    );
                
                Server.AuthenticatedClients.TryRemove(ClientInfo!.User.UserID!, ClientInfo!.User.Username, out _);
#if DEBUG
                Console.WriteLine(ClientInfo?.User.Username + " disconnected.");          
#endif
            }
            
            Server.Clients.Remove(_uuid, out _);
        }

        /// <summary>
        ///     Disconnects client from the server.
        /// </summary>
        public void Disconnect() => 
            _webSocketConnection?.Close();

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
#if DEBUG
            //Console.WriteLine(info);
#endif
            if (version == Version.NotApplicable)
                return (LoginResult.TooOldVersion, null, version);
            
            await using var db = new Database();
            var user = await db.Authenticate(loginUsername, loginPassword);

            if (user == null)
                return (LoginResult.AuthenticationFailed, null, version);
            
            if (user.Banned)
                return (LoginResult.Banned, null, version);

            // user is found, user is not banned, client is not too old. Everything is fine.
            return (LoginResult.AuthenticationSuccessful, user, version);
        }

        private static Version GetProtocol(string clientBuild) => clientBuild switch {
            "1520" => Version.B394A,
            "2000" => Version.B904,
            _ => Version.NotApplicable,
        };
        
        private static readonly ConcurrentDictionary<string, GeoLoc> GeoLocCache = new();
        private static readonly Reader IpLookupDatabase = new("GeoLite2-City.mmdb", FileAccessMode.Memory);
        private static readonly HttpClient HttpClient = new();
        
        private static async Task<(float, float)> GetGeolocationAsync(string ip)
        {
            var data = IpLookupDatabase.Find<Dictionary<string, object>>(IPAddress.Parse(ip));
            
            if (data != null)
            {
                var location = (Dictionary<string, object>)data["location"];
                return ((float)location["latitude"], (float)location["longitude"]);
            }

            if (ip == "127.0.0.1")
                return (0, 0);

            if (GeoLocCache.TryGetValue(ip, out var geoLoc))
                return (geoLoc.Lat, geoLoc.Lon);

            var json = await HttpClient.GetAsync($"http://ip-api.com/json/{ip}");
            geoLoc = JsonConvert.DeserializeObject<GeoLoc>(await json.Content.ReadAsStringAsync());

            GeoLocCache.TryAdd(ip, geoLoc);

            return (geoLoc.Lat, geoLoc.Lon);
        }


    }
}