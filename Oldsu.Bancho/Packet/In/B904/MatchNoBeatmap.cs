using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(55, Version.B904, BanchoPacketType.In)]
    public class MatchNoBeatmap : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchNoBeatmap();
    }
}