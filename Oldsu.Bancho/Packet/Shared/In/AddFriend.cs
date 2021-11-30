using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class AddFriend : ISharedPacketIn
    {
        private readonly uint _userId;

        public AddFriend(uint userId) => this._userId = userId;

        public void Handle(HubEventContext context)
        {
            if (context.Hub.UserPanelManager.IsOnline(_userId))
            {
                Task.Run(async () =>
                {
                    await using var database = new Database();

                    await database.Friends.AddAsync(
                        new Friendship {UserID = _userId, FriendUserID = _userId},
                        context.User.CancellationToken);

                    await database.SaveChangesAsync(context.User.CancellationToken);
                });
            }
        }
    }
}