using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B394a;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacket, Into<IB394APacketOut>
    {
        public Client Client { get; init; }

        public IB394APacketOut Into()
        {
            HandleOsuUpdateOnlineUser packet;

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
                Accuracy = (float)Client.Stats.Accuracy / 100f,
                Rank = 0,
                BStatusUpdate = new bStatusUpdate
                {
                    bStatus = Client.Activity!.Status,
                    BeatmapUpdate = new BeatmapUpdate
                    {
                        Map = Client.Activity.Map,
                        MapSha256 = Client.Activity.MapSHA256,
                        Mods = Client.Activity.Mods,
                    },
                }
            };

            return packet;
        }
    }
}