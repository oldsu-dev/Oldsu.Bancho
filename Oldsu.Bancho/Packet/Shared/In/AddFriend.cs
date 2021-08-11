using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In {
    public class AddFriend : ISharedPacketIn {
        private int _userId;
        public AddFriend(int userId) => this._userId = userId;
        public async Task Handle(UserContext userContext, Connection connection) {
            await using var database = new Database();

            await database.Database.ExecuteSqlRawAsync("INSERT INTO `friends` (UserID, FriendUserID) VALUES ({0}, {1})", userContext.UserID, this._userId);
        }
    }
}
