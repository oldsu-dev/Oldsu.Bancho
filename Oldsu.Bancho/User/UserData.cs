using System;
using Oldsu.Types;

namespace Oldsu.Bancho.User
{
    public class UserData : ICloneable
    {
        public UserInfo UserInfo { get; set; }
        public Presence Presence { get; set; }
        public Activity Activity { get; set; }
        public StatsWithRank? Stats { get; set; }
        
        public object Clone() => MemberwiseClone();
    }
}