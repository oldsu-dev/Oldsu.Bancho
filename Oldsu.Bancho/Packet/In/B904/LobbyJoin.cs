using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(31, Version.B904, BanchoPacketType.In)]
    public struct LobbyJoin : Into<ISharedPacketIn>
    {
        ISharedPacketIn Into<ISharedPacketIn>.Into() => new Shared.In.LobbyJoin();
    }
}