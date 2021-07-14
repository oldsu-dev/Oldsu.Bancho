using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(8, Version.NotApplicable, BanchoPacketType.Out)]
    public class Ping : IGenericPacketOut
    { }
}