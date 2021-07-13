namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacketOut, Into<IB394APacketOut>, Into<IB904PacketOut>
    {
        public Client Client { get; init; }

        IB394APacketOut Into<IB394APacketOut>.Into()
        {
            Packet.Out.B394A.HandleOsuUpdateOnlineUser packet;

            // todo add null check for stats
            packet = new ()
            {
                UserID = (int)Client.User!.UserID,
                Username = Client.User.Username,
                AvatarFilename = "old.jpg",
                Timezone = 0,
                Location = "Poopoo",
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

            return packet;
        }
        
        IB904PacketOut Into<IB904PacketOut>.Into()
        {
            Packet.Out.B904.HandleOsuUpdateOnlineUser packet;

            // todo add null check for stats
            packet = new()
            {
                UserID = (int)Client.User!.UserID,
                Username = Client.User.Username,
                AvatarFilename = "old.jpg",
                Timezone = 0,
                Location = "Poopoo",
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
                        MapId = 0,
                    }
                }
            };

            return packet;
        }
    }
}