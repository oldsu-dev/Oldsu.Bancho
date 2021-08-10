using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(50, Version.B904, BanchoPacketType.In)]
    public struct MatchComplete : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchComplete();
    }
}