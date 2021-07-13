using Oldsu.Bancho.Packet.Out.B394A;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B394a
{
    [BanchoPacket(0, Version.B394A, BanchoPacketType.In)]
    public struct SendUserStatus : Into<ISharedPacketIn>
    {
        [BanchoSerializable] public bStatusUpdate bStatusUpdate;

        ISharedPacketIn Into<ISharedPacketIn>.Into()
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