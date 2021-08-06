using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(61, Version.B904, BanchoPacketType.In)]
    public struct MatchSkip : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchSkip();
    }
}