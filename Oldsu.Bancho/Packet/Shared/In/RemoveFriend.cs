using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In {
    public class RemoveFriend : ISharedPacketIn 
    {
        private int _userId;

        public RemoveFriend(int userId) => this._userId = userId;

        public void Handle(HubEventContext context)
        {
            Task.Run(async () =>
            {
                try
                {
                    await using var database = new Database();

                    var friendship = await database.Friends
                        .Where(friendship => friendship.FriendUserID == _userId
                                             && friendship.UserID == context.User.UserID)
                        .FirstOrDefaultAsync();

                    if (friendship != null)
                    {
                        database.Friends.Remove(friendship);
                        await database.SaveChangesAsync();
                    }
                }
                catch (Exception exception)
                {
                    context.HubEventLoop.SendEvent(new HubEventAsyncError(exception, context.User));
                }
            });
        }
    }
}
