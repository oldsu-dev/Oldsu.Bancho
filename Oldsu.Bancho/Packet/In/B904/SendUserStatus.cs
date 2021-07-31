using Microsoft.EntityFrameworkCore.Diagnostics;
using Oldsu.Bancho.Packet.Out.B904;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(0, Version.B904, BanchoPacketType.In)]
    public struct SendUserStatus : Into<ISharedPacketIn>
    {
        [BanchoSerializable] public bStatusUpdate bStatusUpdate;
        
        ISharedPacketIn Into<ISharedPacketIn>.Into()
        {
            var userActivity = new UserActivity();

            if (bStatusUpdate.BeatmapUpdate is {} beatmapUpdate)
            {
                userActivity.Activity = new ActivityWithBeatmap
                {
                    Action = (Action) bStatusUpdate.bStatus,
                    GameMode = beatmapUpdate.Gamemode,
                    Map = beatmapUpdate.Map,
                    Mods = beatmapUpdate.Mods,
                    MapID = beatmapUpdate.MapId,
                    MapMD5 = beatmapUpdate.MapMD5
                };
            }
            else
            {
                userActivity.Activity = new Activity
                {
                    Action = (Action) bStatusUpdate.bStatus
                };
            }
            
            return userActivity;
        }
    }
}