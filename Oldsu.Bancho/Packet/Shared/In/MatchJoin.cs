using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchJoin : ISharedPacketIn
    {
        public uint MatchID { get; set; }
        public string GamePassword { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            var result = await userContext.LobbyProvider.JoinMatch(userContext.UserID, MatchID, GamePassword);

            if (result is { } matchState)
            {
                await connection.SendPacketAsync(new BanchoPacket(new MatchJoinSuccess {MatchState = matchState}));
                await connection.SendPacketAsync(new BanchoPacket(new ChannelAvailable() {ChannelName = "#multiplayer"}));
            }
            else
                await connection.SendPacketAsync(new BanchoPacket(new MatchJoinFail()));
        }
    }
}