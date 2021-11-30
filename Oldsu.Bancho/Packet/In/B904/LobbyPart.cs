using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(30, Version.B904, BanchoPacketType.In)]
    public struct LobbyPart : IntoPacket<ISharedPacketIn>
    {
        ISharedPacketIn IntoPacket<ISharedPacketIn>.IntoPacket() => new Shared.In.LobbyPart();
    }
}