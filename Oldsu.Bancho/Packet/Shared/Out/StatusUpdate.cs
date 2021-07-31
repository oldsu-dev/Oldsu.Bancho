using Oldsu.Bancho.Enums;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct StatusUpdate : ISharedPacketOut, Into<IB904PacketOut>
    {
        public User User { get; init; }
        public Presence Presence { get; init; }
        public Stats? Stats { get; init; }
        public Activity Activity { get; init; }
        
        public Completeness Completeness { get; init; }

        #region b394a
        /*IB394APacketOut Into<IB394APacketOut>.Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (Stats != null)
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)User.UserID,
                        RankedScore = (long)Stats.RankedScore,
                        TotalScore = (long)Stats.TotalScore,
                        Playcount = (int)Stats.Playcount,
                        Accuracy = (Stats.Accuracy / 100f),
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = Activity.Map,
                                MapMD5 = Activity.MapSHA256,
                                Mods = Activity.Mods,
                            },
                        }
                    };
                }
                else 
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)User.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = Activity.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = Activity.Map,
                                MapMD5 = Activity.MapSHA256,
                                Mods = Activity.Mods,
                            }
                        }
                    };
                }
            }
            else
            {
                if (Stats != null)
                {
                    packet = new HandleOsuUpdateOnlineUser
                    {
                        UserID = (int)User!.UserID,
                        Username = User.Username,
                        AvatarFilename = "23.jpg",
                        Timezone = 0,
                        Location = "ss",
                        RankedScore = (long)Stats!.RankedScore,
                        TotalScore = (long)Stats.TotalScore,
                        Playcount = (int)Stats.Playcount,
                        Accuracy = Stats.Accuracy / 100f,
                        Rank = 0,
                        BStatusUpdate = new bStatusUpdate
                        {
                            bStatus = Activity!.Status,
                            BeatmapUpdate = new BeatmapUpdate
                            {
                                Map = Activity.Map,
                                MapMD5 = Activity.MapSHA256,
                                Mods = Activity.Mods
                            },
                        }
                    };
                }
                else
                {
                    packet = new HandleOsuUpdateOnlineUser
                    {
                        UserID = (int)User!.UserID,
                        Username = User.Username,
                        AvatarFilename = "23.jpg",
                        Timezone = 0,
                        Location = "ss",
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new bStatusUpdate
                        {
                            bStatus = Activity!.Status,
                            BeatmapUpdate = new BeatmapUpdate
                            {
                                Map = Activity.Map,
                                MapSha256 = Activity.MapSHA256,
                                Mods = Activity.Mods
                            },
                        }
                    };
                }
            }
            
            return packet;
        }*/
        
        #endregion
        
        #region b904
        IB904PacketOut Into<IB904PacketOut>.Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (Stats != null) 
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)User.UserID,
                        RankedScore = (long)Stats.RankedScore,
                        TotalScore = (long)Stats.TotalScore,
                        Playcount = (int)Stats.Playcount,
                        Accuracy = (Stats.Accuracy / 100f),
                        Rank = 0,
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
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)User.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0,
                        Rank = 0,
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
            }
            else
            {
                if (Stats != null)
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateOnlineUser()
                    {
                        UserID = (int)User.UserID,
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
                    packet = new Packet.Out.B904.HandleOsuUpdateOnlineUser()
                    {
                        UserID = (int)User.UserID,
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
            }
            
            return packet;
        }
        #endregion b904
    }
}