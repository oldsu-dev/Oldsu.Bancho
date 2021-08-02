using System;
using Oldsu.Types;

namespace Oldsu.Bancho
{
    public class UserData : ICloneable
    {
        public User UserInfo { get; set; }
        public Presence Presence { get; set; }
        public Activity Activity { get; set; }
        public StatsWithRank? Stats { get; set; }
        
        public object Clone() => MemberwiseClone();
    }
}