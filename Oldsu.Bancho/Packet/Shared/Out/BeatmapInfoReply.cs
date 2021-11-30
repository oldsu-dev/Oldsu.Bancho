using System.Collections.Generic;
using System.Linq;
using Oldsu.Bancho.Objects;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class BeatmapInfoReply : SharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public BeatmapInfo[] BeatmapInfos { get; set; }

        public IB904PacketOut IntoPacket() => new Packet.Out.B904.BeatmapInfoReply
        {
            BeatmapInfos = BeatmapInfos.Select(bInfo => new Objects.B904.BeatmapInfo
            {
                Ranked = bInfo.Ranked,
                GradeCatch = bInfo.GradeCatch,
                GradeOsu = bInfo.GradeOsu,
                GradeTaiko = bInfo.GradeTaiko,
                ID = bInfo.ID,
                MapHash = bInfo.MapHash,
                BeatmapID = bInfo.BeatmapID,
                BeatmapsetID = bInfo.BeatmapsetID,
                ThreadID = bInfo.ThreadID
            }).ToList()
        };
    }
}