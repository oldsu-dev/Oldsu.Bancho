namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacket
    {
        public byte Status;
        public string Map;
        public string MapSHA256;
        public ushort Mods;
        public byte Gamemode;
        public int MapID;
    }
}