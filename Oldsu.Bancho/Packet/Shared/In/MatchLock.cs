using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchLock : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public async Task Handle(UserContext userContext, Connection connection)
        {
            var lobbyProvider = userContext.Dependencies.Get<ILobbyProvider>();
            var userRequestProvider = userContext.Dependencies.Get<IUserRequestProvider>();
            
            if ((await lobbyProvider.MatchLockSlot(userContext.UserID, SlotID)) is { } kickUser)
                await userRequestProvider.QuitMatch(kickUser);
        }
    }
}