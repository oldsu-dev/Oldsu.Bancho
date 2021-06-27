using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B394a
{
    [BanchoPacket(0, Version.B394A, BanchoPacketType.In)]
    public struct SendUserStatus : Into<ISharedPacket>
    {
        [BanchoSerializable] private bStatusUpdate bStatusUpdate;

        ISharedPacket Into<ISharedPacket>.Into()
        {
            var userActivity = new UserActivity();

            userActivity.Status = bStatusUpdate.bStatus;
            userActivity.Gamemode = 0;
            userActivity.MapID = 0;
        
            userActivity.Map = bStatusUpdate.BeatmapUpdate?.Map ?? "";
            userActivity.MapSHA256 = bStatusUpdate.BeatmapUpdate?.Map ?? "";
            userActivity.Mods = bStatusUpdate.BeatmapUpdate?.Mods ?? 0;
            
            return userActivity;
        }
    }
}