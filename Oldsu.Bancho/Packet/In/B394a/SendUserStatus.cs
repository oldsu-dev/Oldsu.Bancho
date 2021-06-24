using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Shared;

namespace Oldsu.Bancho.Packet.In.B394a
{
    public struct SendUserStatus : Into<ISharedPacket>
    {
        [BanchoSerializable] public bStatusUpdate bStatusUpdate;

        ISharedPacket Into<ISharedPacket>.Into()
        {
            var userActivity = new UserActivity();

            userActivity.Status = bStatusUpdate.bStatus;
            userActivity.Gamemode = 0;
            userActivity.MapID = 0;
            
            if (bStatusUpdate.BeatmapUpdate)
            {
                userActivity.Map = bStatusUpdate.Map ;
                userActivity.MapSHA256 = bStatusUpdate.MapSha256;
                userActivity.Mods = bStatusUpdate.Mods;
            }
            else
            {
                userActivity.Map = "";
                userActivity.MapSHA256 = "";
                userActivity.Mods = 0;
            }

            return userActivity;
        }
    }
}