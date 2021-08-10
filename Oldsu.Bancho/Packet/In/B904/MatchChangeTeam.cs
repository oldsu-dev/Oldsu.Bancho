using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(78, Version.B904, BanchoPacketType.In)]
    public struct MatchChangeTeam : IntoPacket<ISharedPacketIn>
    {
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchChangeTeam();
    }
}