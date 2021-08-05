using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(60, Version.B904, BanchoPacketType.In)]
    public class MatchGotBeatmap : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchGotBeatmap();
    }
}