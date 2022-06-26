using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

                    uint[] ids = context.Hub.UserPanelManager.Entities
                        .Where(u => (byte)u.User.Stats.Mode == gamemode && u.User.UserID != context.User.UserID)
                        .Select(u => u.User.UserID)
                        .ToArray();

                    var ranks = await database.StatsWithRank.Select(stats => new {stats.UserID, stats.Rank})
                        .Where(stats => ids.Contains(stats.UserID)).ToArrayAsync();
                    
                    context.HubEventLoop.SendEvent(new HubEventAction(context.User,
                        context =>
                        {
                            context.Hub.UserPanelManager.UpdateStats(context.User!, stats!);
                            
                            foreach (var rank in ranks)
                                context.Hub.UserPanelManager.UpdateRank(rank.UserID, rank.Rank);
                        })
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