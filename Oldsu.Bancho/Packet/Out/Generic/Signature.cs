using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(ushort.MaxValue, Oldsu.Enums.Version.NotApplicable, BanchoPacketType.Out)]
    public class Signature : IGenericPacketOut
    {
        [BanchoSerializable()] public string ServerName;
        [BanchoSerializable()] public string Version;
    }
}