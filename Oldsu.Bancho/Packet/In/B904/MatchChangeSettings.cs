using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(42, Version.B904, BanchoPacketType.In)]
    public struct MatchChangeSettings : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable] public MatchState MatchState;
        
        public ISharedPacketIn IntoPacket()
        {
            return new Shared.In.MatchChangeSettings
            {
                MatchSettings = MatchState.ToMatchSettings()
            };
        }
    }
}