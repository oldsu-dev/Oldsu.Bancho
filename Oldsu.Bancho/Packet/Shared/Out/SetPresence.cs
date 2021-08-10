using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacketOut, IntoPacket<IB904PacketOut>
    {
        public UserInfo User { get; init; }
        public Presence Presence { get; init; }
        public StatsWithRank? Stats { get; init; }
        public Activity Activity { get; init; }

        public static SetPresence FromUserData(UserData userData) =>
            new SetPresence
            {
                Activity = userData.Activity,
                Presence = userData.Presence,
                Stats = userData.Stats,
                User = userData.UserInfo
            };
        

        IB904PacketOut IntoPacket<IB904PacketOut>.IntoPacket()
        {
            Packet.Out.B904.HandleOsuUpdateOnlineUser packet;

            if (Stats != null)
            {
                packet = new()
                {
                    UserID = (int)User!.UserID,
                    Username = User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = (long)Stats.RankedScore,
                    TotalScore = (long)Stats.TotalScore,
                    Playcount = (int)Stats.Playcount,
                    Accuracy = Stats.Accuracy / 100f,
                    Rank = 0,
                    Privileges = (byte)Presence.Privilege,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = (byte)Activity.Action,
                        BeatmapUpdate = Activity is ActivityWithBeatmap withBeatmap ?
                            new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = withBeatmap.Map,
                                MapMD5 = withBeatmap.MapMD5,
                                Mods = withBeatmap.Mods,
                                Gamemode = withBeatmap.GameMode,
                                MapId = withBeatmap.MapID,
                            } : null
                    }
                };
            }
            else
            {
                packet = new()
                {
                    UserID = (int)User!.UserID,
                    Username = User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = 0,
                    TotalScore = 0,
                    Playcount = 0,
                    Accuracy = 0 / 100f,
                    Rank = 0,
                    Privileges = (byte)Presence.Privilege,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = (byte)Activity.Action,
                        BeatmapUpdate = Activity is ActivityWithBeatmap withBeatmap ?
                            new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = withBeatmap.Map,
                                MapMD5 = withBeatmap.MapMD5,
                                Mods = withBeatmap.Mods,
                                Gamemode = withBeatmap.GameMode,
                                MapId = withBeatmap.MapID,
                            } : null
                    }
                };
            }

            return packet;
        }
    }
}