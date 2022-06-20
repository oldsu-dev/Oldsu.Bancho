using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Types;

namespace Oldsu.Bancho.GameLogic.Events;

public class HubEventSendFriends : HubEvent
{
    public HubEventSendFriends(User invoker) : base(invoker)
    {
    }

    public override void Handle(HubEventContext context)
    {
        Task.Run(async () =>
        {
            await using var db = new Database();
            List<Friendship> friendships = await db.Friends.Where(f => f.UserID == context.User.UserID)
                .ToListAsync(context.User!.CancellationToken);
            
            context.User.SendPacket(new BanchoFriendsList{Friendships = friendships});
        });
    }
}