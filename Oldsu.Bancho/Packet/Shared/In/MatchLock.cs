using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLock : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            if ((await userContext.LobbyProvider.MatchLockSlot(userContext.UserID, SlotID)) is { } kickUser)
                await userContext.UserRequestProvider.QuitMatch(kickUser);
        }
    }
}