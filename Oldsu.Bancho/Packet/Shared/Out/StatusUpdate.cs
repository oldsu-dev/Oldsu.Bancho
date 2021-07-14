using Oldsu.Bancho.Enums;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct StatusUpdate : ISharedPacketOut, Into<IB394APacketOut>, Into<IB904PacketOut>
    {
        public ClientInfo ClientInfo { get; init; }
        public Completeness Completeness { get; init; }

        IB394APacketOut Into<IB394APacketOut>.Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (ClientInfo.Stats != null)
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)ClientInfo.User!.UserID,
                        RankedScore = (long)ClientInfo.Stats!.RankedScore,
                        TotalScore = (long)ClientInfo.Stats.TotalScore,
                        Playcount = (int)ClientInfo.Stats.Playcount,
                        Accuracy = (ClientInfo.Stats.Accuracy / 100f),
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = ClientInfo.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = ClientInfo.Activity.Map,
                                MapSha256 = ClientInfo.Activity.MapSHA256,
                                Mods = ClientInfo.Activity.Mods,
                            },
                        }
                    };
                }
                else 
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)ClientInfo.User!.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = ClientInfo.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = ClientInfo.Activity.Map,
                                MapSha256 = ClientInfo.Activity.MapSHA256,
                                Mods = ClientInfo.Activity.Mods,
                            }
                        }
                    };
                }
            }
            else
            {
                packet = new Packet.Out.B394A.HandleOsuUpdateOnlineUser
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "23.jpg",
                    Timezone = 0,
                    Location = "ss",
                    RankedScore = (long)ClientInfo.Stats!.RankedScore,
                    TotalScore = (long)ClientInfo.Stats.TotalScore,
                    Playcount = (int)ClientInfo.Stats.Playcount,
                    Accuracy = ClientInfo.Stats.Accuracy / 100f,
                    Rank = 0,
                    BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                    {
                        bStatus = ClientInfo.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                        {
                            Map = ClientInfo.Activity.Map,
                            MapSha256 = ClientInfo.Activity.MapSHA256,
                            Mods = ClientInfo.Activity.Mods
                        },
                    }
                };
            }
            
            return packet;
        }
        
        IB904PacketOut Into<IB904PacketOut>.Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (ClientInfo.Stats != null) 
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)ClientInfo.User!.UserID,
                        RankedScore = (long)ClientInfo.Stats.RankedScore,
                        TotalScore = (long)ClientInfo.Stats.TotalScore,
                        Playcount = (int)ClientInfo.Stats.Playcount,
                        Accuracy = (ClientInfo.Stats.Accuracy / 100f),
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                        {
                            bStatus = ClientInfo.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = ClientInfo.Activity.Map,
                                MapSha256 = ClientInfo.Activity.MapSHA256,
                                Mods = ClientInfo.Activity.Mods,
                                Gamemode = ClientInfo.Activity.GameMode,
                                MapId = ClientInfo.Activity.MapID,
                            }
                        }
                    };
                }
                else 
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)ClientInfo.User!.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                        {
                            bStatus = ClientInfo.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = ClientInfo.Activity.Map,
                                MapSha256 = ClientInfo.Activity.MapSHA256,
                                Mods = ClientInfo.Activity.Mods,
                                Gamemode = ClientInfo.Activity.GameMode,
                                MapId = ClientInfo.Activity.MapID,
                            }
                        }
                    };
                }
            }
            else
            {
                packet = new Packet.Out.B904.HandleOsuUpdateOnlineUser
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "23.jpg",
                    Timezone = 0,
                    Location = "ss",
                    RankedScore = (long)ClientInfo.Stats!.RankedScore,
                    TotalScore = (long)ClientInfo.Stats.TotalScore,
                    Playcount = (int)ClientInfo.Stats.Playcount,
                    Accuracy = ClientInfo.Stats.Accuracy / 100f,
                    Rank = 0,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = ClientInfo.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                        {
                            Map = ClientInfo.Activity.Map,
                            MapSha256 = ClientInfo.Activity.MapSHA256,
                            Mods = ClientInfo.Activity.Mods,
                            Gamemode = ClientInfo.Activity.GameMode,
                            MapId = ClientInfo.Activity.MapID,
                        }
                    }
                };
            }
            
            return packet;
        }
    }
}