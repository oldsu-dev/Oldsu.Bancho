using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(17, Version.NotApplicable, BanchoPacketType.In)]
    public struct StartSpectating : IntoPacket<Shared.In.StartSpectating>
    {
        [BanchoSerializable] public int UserID;
        
        public Shared.In.StartSpectating IntoPacket()
        {
            return new Shared.In.StartSpectating
            {
                UserID = UserID
            };
        }
    }
}