using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchPlayerTransferHost : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            var newHost = await userContext.LobbyProvider.MatchTransferHost(userContext.UserID, SlotID);
            await userContext.UserRequestProvider.AnnounceTransferHost(newHost);
        }
    }
}