using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Enums;

namespace Oldsu.Bancho.Spectating
{
    public class GameSpectator
    {
        public OnlineUser? SpectatingUser { get; set; }
    }

    public class GameBroadcaster
    {
        public GameBroadcaster()
        {
            Spectators = new Dictionary<uint, OnlineUser>();
        }
        
        public Dictionary<uint, OnlineUser> Spectators { get; }
    }
}