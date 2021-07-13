using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B394A;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacketOut, Into<IB394APacketOut>
    {
        public Client Client { get; init; }

        public IB394APacketOut Into()
        {
            HandleOsuUpdateOnlineUser packet;

            // todo add null check for stats
            packet = new HandleOsuUpdateOnlineUser
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
                BStatusUpdate = new bStatusUpdate
                {
                    bStatus = Client.Activity!.Status,
                    BeatmapUpdate = new BeatmapUpdate
                    {
                        Map = Client.Activity.Map,
                        MapSha256 = Client.Activity.MapSHA256,
                        Mods = Client.Activity.Mods
                    },
                }
            };

            return packet;
        }
    }
}