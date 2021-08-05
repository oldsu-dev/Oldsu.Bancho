using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(1, Version.NotApplicable, BanchoPacketType.In)]
    public struct SendMessage : IntoPacket<Shared.In.SendMessage>
    {
        [BanchoSerializable] public string _;
        [BanchoSerializable] public string Contents;
        [BanchoSerializable] public string Target;
        
        public Shared.In.SendMessage IntoPacket() => new () { Contents = Contents, Target = Target};
    }
}