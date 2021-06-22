using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fleck;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Oldsu.Types;
using osuserver2012.Enums;

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

        public Client(IWebSocketConnection webSocketConnection)
        {
            _webSocketConnection = webSocketConnection;
        }

        /// <summary>
        ///     Handles incoming login requests accordingly.
        /// </summary>
        /// <param name="authenticationString"> Authentication string that osu! sends on login. </param>
        public async Task HandleLoginAsync(string authenticationString)
        {
            var (loginStatus, user) = await AuthenticateAsync(authenticationString.Replace("\r", "").Split("\n"));
        }

        /// <summary>
        ///     Returns the authentication result of the user.
        /// </summary>
        /// <param name="authenticationString"> Authentication string seperated by \n </param>
        /// <returns> Result of the authentication and the User variable, if the authentication was successful </returns>
        private static async Task<(LoginResult, User)> AuthenticateAsync(IReadOnlyList<string> authenticationString)
        {
            var (loginUsername, loginPassword, info) =
                (authenticationString[0], authenticationString[1], authenticationString[2]);

            if (info.Split("|")[0] != "1520") // todo version number thing
                return (LoginResult.TooOldVersion, null);
            
            await using var db = new Database();

            var user = await db.Users.Where(user => user.Username == loginUsername).FirstAsync();

            if (user.Banned == true)
                return (LoginResult.Banned, null);

            if (user.Password != loginPassword) 
                return (LoginResult.AuthenticationFailed, null);
            
            // Password is correct, user is not banned, client is not too old. Everything is fine.
            return (LoginResult.AuthenticationSuccessful, user);
        }

        
        
        private static async Task<(float, float)> RetrieveGeoLocationAsync(string ip)
        {
            var json = await new HttpClient().GetAsync($"http://ip-api.com/json/{ip}");

            var geoLoc = JsonConvert.DeserializeObject<GeoLocSerialization>(await json.Content.ReadAsStringAsync());

            return (geoLoc.Lat, geoLoc.Lon);
        }
    }
}