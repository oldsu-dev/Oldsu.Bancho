using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLock : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public Task Handle(UserContext userContext, Connection connection) =>
            userContext.LobbyProvider.MatchLockSlot(userContext.UserID, SlotID);
    }
}