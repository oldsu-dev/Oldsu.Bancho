using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(30, Version.B904, BanchoPacketType.In)]
    public struct LobbyPart : Into<ISharedPacketIn>
    {
        ISharedPacketIn Into<ISharedPacketIn>.Into() => new Shared.In.LobbyPart();
    }
}