namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct StatusUpdate : ISharedPacketOut, Into<IB394APacketOut>, Into<IB904PacketOut>
    {
        public Client Client { get; init; }
        public Completeness Completeness { get; init; }

        IB394APacketOut Into<IB394APacketOut>.Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (Client.Stats != null)
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = (long)Client.Stats.RankedScore,
                        TotalScore = (long)Client.Stats.TotalScore,
                        Playcount = (int)Client.Stats.Playcount,
                        Accuracy = (Client.Stats.Accuracy / 100f),
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = Client.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = Client.Activity.Map,
                                MapSha256 = Client.Activity.MapSHA256,
                                Mods = Client.Activity.Mods,
                            },
                        }
                    };
                }
                else 
                {
                    packet = new Packet.Out.B394A.HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                        {
                            bStatus = Client.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                            {
                                Map = Client.Activity.Map,
                                MapSha256 = Client.Activity.MapSHA256,
                                Mods = Client.Activity.Mods,
                            }
                        }
                    };
                }
            }
            else
            {
                packet = new Packet.Out.B394A.HandleOsuUpdateOnlineUser
                {
                    UserID = (int)Client.User!.UserID,
                    Username = Client.User.Username,
                    AvatarFilename = "23.jpg",
                    Timezone = 0,
                    Location = "ss",
                    RankedScore = (long)Client.Stats!.RankedScore,
                    TotalScore = (long)Client.Stats.TotalScore,
                    Playcount = (int)Client.Stats.Playcount,
                    Accuracy = Client.Stats.Accuracy / 100f,
                    Rank = 0,
                    BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                    {
                        bStatus = Client.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                        {
                            Map = Client.Activity.Map,
                            MapSha256 = Client.Activity.MapSHA256,
                            Mods = Client.Activity.Mods
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
                if (Client.Stats != null) 
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = (long)Client.Stats.RankedScore,
                        TotalScore = (long)Client.Stats.TotalScore,
                        Playcount = (int)Client.Stats.Playcount,
                        Accuracy = (Client.Stats.Accuracy / 100f),
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                        {
                            bStatus = Client.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = Client.Activity.Map,
                                MapSha256 = Client.Activity.MapSHA256,
                                Mods = Client.Activity.Mods,
                                Gamemode = Client.Activity.Gamemode,
                                MapId = Client.Activity.MapID,
                            }
                        }
                    };
                }
                else 
                {
                    packet = new Packet.Out.B904.HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                        {
                            bStatus = Client.Activity!.Status,
                            BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                            {
                                Map = Client.Activity.Map,
                                MapSha256 = Client.Activity.MapSHA256,
                                Mods = Client.Activity.Mods,
                                Gamemode = Client.Activity.Gamemode,
                                MapId = Client.Activity.MapID,
                            }
                        }
                    };
                }
            }
            else
            {
                packet = new Packet.Out.B904.HandleOsuUpdateOnlineUser
                {
                    UserID = (int)Client.User!.UserID,
                    Username = Client.User.Username,
                    AvatarFilename = "23.jpg",
                    Timezone = 0,
                    Location = "ss",
                    RankedScore = (long)Client.Stats!.RankedScore,
                    TotalScore = (long)Client.Stats.TotalScore,
                    Playcount = (int)Client.Stats.Playcount,
                    Accuracy = Client.Stats.Accuracy / 100f,
                    Rank = 0,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = Client.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                        {
                            Map = Client.Activity.Map,
                            MapSha256 = Client.Activity.MapSHA256,
                            Mods = Client.Activity.Mods,
                            Gamemode = Client.Activity.Gamemode,
                            MapId = Client.Activity.MapID,
                        }
                    }
                };
            }
            
            return packet;
        }
    }
}