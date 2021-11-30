using Oldsu.Enums;

namespace Oldsu.Bancho.Objects
{
    public class BeatmapInfo
    {
        public ushort ID { get; set; }
        public int BeatmapID { get; set; }
        public int BeatmapsetID { get; set; }
        public int ThreadID { get; set; }
        public bool Ranked { get; set; }
        public byte GradeOsu { get; set; } = (byte)Rankings.N;
        public byte GradeTaiko { get; set; } = (byte)Rankings.N;
        public byte GradeCatch { get; set; } = (byte)Rankings.N;
        public byte GradeMania { get; set; } = (byte)Rankings.N;
        public string MapHash { get; set; }
    }
}