using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(26, Version.NotApplicable, BanchoPacketType.In)]
    public struct SendPrivateMessage : Into<Shared.In.SendPrivateMessage>, IGenericPacketIn
    {
        [BanchoSerializable] public string _;
        [BanchoSerializable] public string Contents;
        [BanchoSerializable] public string Target;
        
        public Shared.In.SendPrivateMessage Into() => new () { Contents = Contents, Target = Target};
    }
}