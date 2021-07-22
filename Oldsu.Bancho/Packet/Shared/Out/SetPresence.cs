namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacketOut, Into<IB394APacketOut>, Into<IB904PacketOut>
    {
        public ClientContext ClientContext { get; init; }

        IB394APacketOut Into<IB394APacketOut>.Into()
        {
            Packet.Out.B394A.HandleOsuUpdateOnlineUser packet;

            // todo add null check for stats
            packet = new ()
            {
                UserID = (int)ClientContext.User!.UserID,
                Username = ClientContext.User.Username,
                AvatarFilename = "old.jpg",
                Timezone = 0,
                Location = "Poopoo",
                RankedScore = (long)ClientContext.Stats!.RankedScore,
                TotalScore = (long)ClientContext.Stats.TotalScore,
                Playcount = (int)ClientContext.Stats.Playcount,
                Accuracy = ClientContext.Stats.Accuracy / 100f,
                Rank = 0,
                BStatusUpdate = new Packet.Out.B394A.bStatusUpdate
                {
                    bStatus = ClientContext.Activity!.Status,
                    BeatmapUpdate = new Packet.Out.B394A.BeatmapUpdate
                    {
                        Map = ClientContext.Activity.Map,
                        MapSha256 = ClientContext.Activity.MapSHA256,
                        Mods = ClientContext.Activity.Mods
                    },
                }
            };

            return packet;
        }
        
        IB904PacketOut Into<IB904PacketOut>.Into()
        {
            Packet.Out.B904.HandleOsuUpdateOnlineUser packet;

            if (ClientContext.Stats != null)
            {
                packet = new()
                {
                    UserID = (int)ClientContext.User!.UserID,
                    Username = ClientContext.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = (long)ClientContext.Stats!.RankedScore,
                    TotalScore = (long)ClientContext.Stats.TotalScore,
                    Playcount = (int)ClientContext.Stats.Playcount,
                    Accuracy = ClientContext.Stats.Accuracy / 100f,
                    Rank = 0,
                    Privileges = (byte)ClientContext.Presence.Privilege,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = ClientContext.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                        {
                            Map = ClientContext.Activity.Map,
                            MapSha256 = ClientContext.Activity.MapSHA256,
                            Mods = ClientContext.Activity.Mods,
                            Gamemode = ClientContext.Activity.GameMode,
                            MapId = 0,
                        }
                    }
                };
            }
            else
            {
                packet = new ()
                {
                    UserID = (int)ClientContext.User!.UserID,
                    Username = ClientContext.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = 0,
                    TotalScore = 0,
                    Playcount = 0,
                    Accuracy = 0 / 100f,
                    Rank = 0,
                    Privileges = (byte)ClientContext.Presence.Privilege,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = ClientContext.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                        {
                            Map = ClientContext.Activity.Map,
                            MapSha256 = ClientContext.Activity.MapSHA256,
                            Mods = ClientContext.Activity.Mods,
                            Gamemode = ClientContext.Activity.GameMode,
                            MapId = ClientContext.Activity.MapID,
                        }
                    }
                };
            }

            return packet;
        }
    }
}