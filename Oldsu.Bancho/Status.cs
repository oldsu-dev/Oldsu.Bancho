namespace Oldsu.Bancho
{
    public class Status
    {
        public byte bStatus { get; set; }
        public byte Gamemode { get; set; }
        public ushort Mods { get; set; }
        public string Map { get; set; }
        public string MapSha256 { get; set; }
        public int MapID { get; set; }
        
        public Status()
        {
            bStatus = 0;
            Gamemode = 0;
            Mods = 0;
            Map = "";
            MapSha256 = "";
            MapID = 0;
        }
    }
}