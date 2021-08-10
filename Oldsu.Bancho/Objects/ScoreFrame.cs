namespace Oldsu.Bancho.Objects
{
    public class ScoreFrame 
    {
        public int Time { get; set; }
        public byte SlotID { get; set; } 
        public ushort Count300 { get; set; }
        public ushort Count100 { get; set; }
        public ushort Count50 { get; set; }
        public ushort CountGeki { get; set; }
        public ushort CountKatu { get; set; }
        public ushort CountMiss { get; set; }
        public int Score { get; set; }
        public ushort MaxCombo { get; set; }
        public ushort CurrentCombo { get; set; }
        public bool Perfect { get; set; }
        public byte CurrentHealth { get; set; }
        public byte TagByte { get; set; }
        
        // 20 seconds wait time.........................................
        
        public static ScoreFrame FromB904ScoreFrame(Packet.Objects.B904.ScoreFrame srcScoreFrame)
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