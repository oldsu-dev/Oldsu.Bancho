using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Objects.B904
{
    public class BeatmapInfo
    {
        [BanchoSerializable()]
        public ushort ID;
        [BanchoSerializable()]
        public int BeatmapID;
        [BanchoSerializable()]
        public int BeatmapsetID;
        [BanchoSerializable()]
        public int ThreadID;
        [BanchoSerializable()]
        public bool Ranked;
        [BanchoSerializable()]
        public byte GradeOsu = (byte)Rankings.N;
        [BanchoSerializable()]
        public byte GradeTaiko = (byte)Rankings.N;
        [BanchoSerializable()]
        public byte GradeCatch = (byte)Rankings.N;
        [BanchoSerializable()]
        public string MapHash;
    }
}