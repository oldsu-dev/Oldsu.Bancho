namespace Oldsu.Bancho.Objects
{
    public struct bStatusUpdate
    {
        [BanchoSerializable] public byte bStatus;
        [BanchoSerializable] public bool BeatmapUpdate;
        [BanchoSerializable] public string Map;
        [BanchoSerializable] public string MapSha256;
        [BanchoSerializable] public ushort Mods;
    }
}