namespace Oldsu.Bancho
{
    public class Status
    {
        public byte bStatus { get; set; }
        public byte Gamemode { get; set; }
        public ushort Mods { get; set; }
        public string Map { get; set; }
        public string MapMD5 { get; set; }
        public int MapID { get; set; }
        
        public Status()
        {
            bStatus = 0;
            Gamemode = 0;
            Mods = 0;
            Map = "";
            MapMD5 = "";
            MapID = 0;
        }
    }
}