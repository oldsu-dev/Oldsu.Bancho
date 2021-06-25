using System.Data;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B394a;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared
{
    public struct SetPresence : ISharedPacket, Into<IB394APacketOut>
    {
        public Client Client { get; init; }

        public IB394APacketOut Into()
        {
            HandleOsuUpdateOnlineUser packet;

            packet = new HandleOsuUpdateOnlineUser
            {
                UserID = (int)Client.User.UserID,
                Username = Client.User.Username,
                AvatarFilename = "ezchamp_old.jpg",
                Timezone = 0,
                Location = "Poopoo",
                RankedScore = (long)Client.Stats.RankedScore,
                TotalScore = (long)Client.Stats.TotalScore,
                Playcount = (int)Client.Stats.Playcount,
                Accuracy = Client.Stats.Accuracy / 100f,
                Rank = 0,
                BStatusUpdate = new bStatusUpdate
                {
                    bStatus = Client.Status.bStatus,
                    BeatmapUpdate = true,
                    Map = Client.Status.Map,
                    MapSha256 = Client.Status.MapSha256,
                    Mods = Client.Status.Mods,
                }
            };

            return packet;
        }
    }
}