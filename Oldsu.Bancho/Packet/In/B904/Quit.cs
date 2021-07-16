using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(2, Version.B904, BanchoPacketType.In)]
    public struct Quit : Into<Shared.In.Quit>
    {
        Shared.In.Quit Into<Shared.In.Quit>.Into() => new();
    }
}