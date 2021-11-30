using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(31, Version.B904, BanchoPacketType.In)]
    public struct LobbyJoin : IntoPacket<ISharedPacketIn>
    {
        ISharedPacketIn IntoPacket<ISharedPacketIn>.IntoPacket() => new Shared.In.LobbyJoin();
    }
}