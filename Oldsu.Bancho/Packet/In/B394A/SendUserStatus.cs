using Oldsu.Bancho.Packet.Out.B394A;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B394A
{
    [BanchoPacket(0, Version.B394A, BanchoPacketType.In)]
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
                    GameMode = 0,
                    Map = beatmapUpdate.Map,
                    Mods = beatmapUpdate.Mods,
                    MapID = 0,
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