using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            var match = await userContext.Dependencies.Get<ILobbyProvider>()
                .CreateMatch(userContext.UserID, MatchSettings);
            
            if (match is null)
                await connection.SendPacketAsync(new BanchoPacket(new MatchJoinFail()));
            else
            {
                await connection.SendPacketAsync(new BanchoPacket(new MatchJoinSuccess {MatchState = match}));
                await connection.SendPacketAsync(new BanchoPacket(new ChannelAvailable() {ChannelName = "#multiplayer"}));
            }
        }
    }
}