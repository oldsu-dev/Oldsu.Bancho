using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Shared;

namespace Oldsu.Bancho.Packet.In.B394a
{
    public struct SendUserStatus : Into<ISharedPacket>
    {
        [BanchoSerializable] public byte Status;
        [BanchoSerializable] public bool IsBeatmapUpdate;
        [BanchoSerializable] public BeatmapUpdate BeatmapUpdate;

        ISharedPacket Into<ISharedPacket>.Into()
        {
            var userActivity = new UserActivity();

            userActivity.Status = Status;
            userActivity.Gamemode = 0;
            userActivity.MapID = 0;
            
            if (IsBeatmapUpdate)
            {
                userActivity.Map = BeatmapUpdate.Map ;
                userActivity.MapSHA256 = BeatmapUpdate.MapSha256;
                userActivity.Mods = BeatmapUpdate.Mods;
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