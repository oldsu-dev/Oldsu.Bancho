namespace Oldsu.Bancho.Packet.Objects.B904
{
    public class ScoreFrame
    {
        [BanchoSerializable()] public int Time;
        [BanchoSerializable()] public byte SlotID;
        [BanchoSerializable()] public ushort Count300;
        [BanchoSerializable()] public ushort Count100;
        [BanchoSerializable()] public ushort Count50;
        [BanchoSerializable()] public ushort CountGeki;
        [BanchoSerializable()] public ushort CountKatu;
        [BanchoSerializable()] public ushort CountMiss;
        [BanchoSerializable()] public int Score;
        [BanchoSerializable()] public ushort MaxCombo;
        [BanchoSerializable()] public ushort CurrentCombo;
        [BanchoSerializable()] public bool Perfect;
        [BanchoSerializable()] public byte CurrentHealth;
        [BanchoSerializable()] public byte TagByte;

        public static ScoreFrame FromSharedScoreFrame(Bancho.Objects.ScoreFrame srcScoreFrame)
        {
            var destScoreFrame = new ScoreFrame
            {
                Time = srcScoreFrame.Time,
                SlotID = srcScoreFrame.SlotID,
                Count300 = srcScoreFrame.Count300,
                Count100 = srcScoreFrame.Count100,
                Count50 = srcScoreFrame.Count50,
                CountGeki = srcScoreFrame.CountGeki,
                CountKatu = srcScoreFrame.CountKatu,
                CountMiss = srcScoreFrame.CountMiss,
                Score = srcScoreFrame.Score,
                MaxCombo = srcScoreFrame.MaxCombo,
                CurrentCombo = srcScoreFrame.CurrentCombo,
                Perfect = srcScoreFrame.Perfect,
                CurrentHealth = srcScoreFrame.CurrentHealth,
                TagByte = srcScoreFrame.TagByte
            };

            return destScoreFrame;
        }
    }
}