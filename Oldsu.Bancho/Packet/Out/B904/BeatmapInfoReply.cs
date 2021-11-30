using System.Collections.Generic;
using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(70, Version.B904, BanchoPacketType.Out)]
    public class BeatmapInfoReply : IB904PacketOut
    {
        [BanchoSerializable] public List<BeatmapInfo> BeatmapInfos;
    }
}