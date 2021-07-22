using Oldsu.Bancho.Packet.Objects.B904;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(42, Version.B904, BanchoPacketType.In)]
    public struct MatchChangeSettings : Into<ISharedPacketIn>
    {
        [BanchoSerializable] public Match Match;
        
        public ISharedPacketIn Into()
        {
            return new Shared.In.MatchChangeSettings
            {
                GamePassword = Match.GamePassword,
                BeatmapChecksum = Match.BeatmapChecksum,
                BeatmapName = Match.BeatmapName,
                GameName = Match.GameName,
                BeatmapID = Match.BeatmapID,
                ActiveMods = Match.MatchID,
                MatchType = Match.MatchType
            };
        }
    }
}