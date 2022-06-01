using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public void Handle(HubEventContext context)
        {
            var gamemode = context.User!.Activity is ActivityWithBeatmap activityWithBeatmap
                ? activityWithBeatmap.GameMode : (byte)Mode.Standard;

            Task.Run(async () =>
            {
                try
                {
                    await using var database = new Database();

                    var stats = await database.GetStatsWithRankAsync(
                        context.User.UserID, gamemode, context.User.CancellationToken);

                    context.HubEventLoop.SendEvent(new HubEventAction(context.User,
                        context => context.Hub.UserPanelManager.UpdateStats(context.User, stats))
                    );
                }
                catch (Exception exception)
                {
                    context.HubEventLoop.SendEvent(new HubEventAsyncError(exception, context.User));
                }
            });
        }
    }
}