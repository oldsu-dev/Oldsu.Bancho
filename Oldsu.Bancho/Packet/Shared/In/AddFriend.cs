using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.PacketHandling;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class AddFriend : ISharedPacketIn
    {
        private readonly uint _userId;

        public AddFriend(uint userId) => this._userId = userId;

        public void Handle(HubEventContext context)
        {
            if (_userId == context.User!.UserID)
                throw new SelfFriendAttemptException();
            
            if (context.Hub.UserPanelManager.IsOnline(_userId))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await using var database = new Database();

                        await database.Friends.AddAsync(
                            new Friendship {UserID = context.User!.UserID, FriendUserID = _userId},
                            context.User.CancellationToken);

                        await database.SaveChangesAsync(context.User.CancellationToken);
                    }
                    catch (Exception exception)
                    {
                        context.HubEventLoop.SendEvent(new HubEventAsyncError(exception, context.User!));
                    }
                });
            }
        }
    }
}