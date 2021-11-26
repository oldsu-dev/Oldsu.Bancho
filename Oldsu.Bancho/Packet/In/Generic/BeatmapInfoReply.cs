using System.Collections.Generic;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Objects.B904
{
    [BanchoPacket(69, Version.NotApplicable, BanchoPacketType.In)]
    public struct BeatmapInfoRequest : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable()]
        public List<string> RawInfo;

        public ISharedPacketIn IntoPacket() => new Shared.In.BeatmapInfoRequest();
    }
}
