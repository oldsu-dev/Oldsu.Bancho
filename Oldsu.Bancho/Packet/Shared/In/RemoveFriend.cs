using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In {
    public class RemoveFriend : ISharedPacketIn {
        private int _userId;

        public RemoveFriend(int userId) => this._userId = userId;

        public async Task Handle(UserContext userContext, Connection connection) {
            await using var database = new Database();

            Friendship removedFriendship = new Friendship() {
                UserID       = userContext.UserID,
                FriendUserID = (uint) this._userId
            };

            database.Friends.Attach(removedFriendship);
            database.Friends.Remove(removedFriendship);

            await database.SaveChangesAsync();
        }
    }
}
