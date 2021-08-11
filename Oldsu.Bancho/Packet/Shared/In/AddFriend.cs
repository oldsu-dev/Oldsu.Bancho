using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In {
    public class AddFriend : ISharedPacketIn {
        private int _userId;
        public AddFriend(int userId) => this._userId = userId;
        public async Task Handle(UserContext userContext, Connection connection) {
            await using var database = new Database();

            await database.Friends.AddAsync(new Friendship() {
                UserID       = userContext.UserID,
                FriendUserID = (uint) this._userId
            });
        }
    }
}
