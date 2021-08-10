using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(26, Version.NotApplicable, BanchoPacketType.In)]
    public struct SendPrivateMessage : IntoPacket<Shared.In.SendPrivateMessage>
    {
        [BanchoSerializable] public string _;
        [BanchoSerializable] public string Contents;
        [BanchoSerializable] public string Target;
        
        public Shared.In.SendPrivateMessage IntoPacket() => new () { Contents = Contents, Target = Target};
    }
}