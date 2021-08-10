using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchChangeSlot : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchMoveSlot(userContext.UserID, SlotID);
    }
}