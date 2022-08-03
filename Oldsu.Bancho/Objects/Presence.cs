using Oldsu.Enums;

namespace Oldsu.Bancho
{
    public class Presence
    {
        public Privileges Privilege { get; set; }
        
        public byte UtcOffset { get; set; }
        public byte Country { get; set; }
        
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }
}