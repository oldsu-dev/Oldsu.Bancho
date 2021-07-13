namespace Oldsu.Bancho.Packet.Out.B394A
{
    public struct BeatmapUpdate
    {
        [BanchoSerializable] public string Map;
        [BanchoSerializable] public string MapSha256;
        [BanchoSerializable] public ushort Mods;
    }
    
    public class bStatusUpdate
    {
        [BanchoSerializable] public byte bStatus;
        
        [BanchoSerializable(optional: true)]
        public BeatmapUpdate? BeatmapUpdate;
    }
}