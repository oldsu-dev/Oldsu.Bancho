using Oldsu.Enums;

namespace Oldsu.Bancho
{
    public class Activity
    {
        public Action Action { get; init; }
    }
    
    public class ActivityWithBeatmap : Activity
    {
        public string Map { get; init; }
        public string MapMD5 { get; init; }
        public ushort Mods { get; init; }
        public byte GameMode { get; init; }
        public int MapID { get; init; }
    }
}