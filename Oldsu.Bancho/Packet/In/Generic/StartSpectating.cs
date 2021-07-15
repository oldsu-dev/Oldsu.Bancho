using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(17, Version.NotApplicable, BanchoPacketType.In)]
    public struct StartSpectating : Into<Shared.In.StartSpectating>, IGenericPacketIn
    {
        [BanchoSerializable] public int UserID;
        
        public Shared.In.StartSpectating Into()
        {
            return new Shared.In.StartSpectating
            {
                UserID = UserID
            };
        }
    }
}