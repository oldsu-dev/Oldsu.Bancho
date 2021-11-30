namespace Oldsu.Bancho.Packet.Out.B904
{
    public struct BeatmapUpdate
    {
        [BanchoSerializable] public string Map;
        [BanchoSerializable] public string MapMD5;
        [BanchoSerializable] public ushort Mods;
        [BanchoSerializable] public byte Gamemode;
        [BanchoSerializable] public int MapId;
    }
    
    public class bStatusUpdate
    {
        [BanchoSerializable] public byte bStatus;
        
        [BanchoSerializable(optional: true)]
        public BeatmapUpdate? BeatmapUpdate;
    }
}