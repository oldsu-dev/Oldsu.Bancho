using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(1, Version.NotApplicable, BanchoPacketType.In)]
    public struct SendMessage : Into<Shared.In.SendMessage>, IGenericPacketIn
    {
        [BanchoSerializable] public string _;
        [BanchoSerializable] public string Contents;
        [BanchoSerializable] public string Target;
        
        public Shared.In.SendMessage Into() => new () { Contents = Contents, Target = Target};
    }
}