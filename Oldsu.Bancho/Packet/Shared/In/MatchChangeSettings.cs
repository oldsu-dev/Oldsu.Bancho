using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Multiplayer.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSettings : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }
        
        public async Task Handle(OnlineUser self)
        {
            if (self.MatchMediator is {} matchMediator)
            {
                await matchMediator.CurrentMatch.WriteAsync(match =>
                {
                    if (match.HostID != self.UserInfo.UserID)
                        return;

                    match.ChangeSettings(MatchSettings);
                });
            }
        }
    }
}