using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.Generic
{
    [BanchoPacket(7, Version.NotApplicable, BanchoPacketType.Out)]
    public class SendMessage : IGenericPacketOut
    {
        [BanchoSerializable] public string Sender;
        [BanchoSerializable] public string Contents;
        [BanchoSerializable] public string Target;
    }
}