using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(45, Version.B904, BanchoPacketType.In)]
    public struct MatchStart : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchStart();
    }
}