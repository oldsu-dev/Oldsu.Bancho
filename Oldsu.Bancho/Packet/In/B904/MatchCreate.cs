using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(32, Version.B904, BanchoPacketType.In)]
    public struct MatchCreate : Into<ISharedPacketIn>
    {
        [BanchoSerializable] public Match Match;
        
        public ISharedPacketIn Into()
        {
            return new Shared.In.MatchCreate
            {
                MatchSettings = Match.ToMatchSettings()
            };
        }
    }
}