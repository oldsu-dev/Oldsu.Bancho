using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Multiplayer.Enums;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSettings : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }
        
        public async Task Handle(UserContext userContext, Connection _)
        {
            /*if (self.MatchMediator is {} matchMediator)
            {
                await matchMediator.CurrentMatch.WriteAsync(match =>
                {
                    if (match.HostID != self.UserInfo.UserID)
                        return;

                    match.ChangeSettings(MatchSettings);
                });
            }*/
        }
    }
}