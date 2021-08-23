using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class AddFriend : ISharedPacketIn
    {
        private uint _userId;

        public AddFriend(uint userId) => this._userId = userId;

        public async Task Handle(UserContext userContext, Connection connection)
        {
            if (await userContext.Dependencies.Get<IUserStateProvider>().IsUserOnline(_userId))
            {
                await using var database = new Database();

                await database.Friends.AddAsync(new Friendship {UserID = userContext.UserID, FriendUserID = _userId});
                await database.SaveChangesAsync();
            }
        }
    }
}