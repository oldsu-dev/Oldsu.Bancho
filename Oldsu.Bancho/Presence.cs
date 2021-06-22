using Oldsu.Enums;

namespace Oldsu.Bancho
{
    public class Presence
    {
        public Privileges Privilege;
        
        public byte UtcOffset;
        public byte Country; // find out is this byte or string
        
        public float Longitude;
        public float Latitude;
    }
}