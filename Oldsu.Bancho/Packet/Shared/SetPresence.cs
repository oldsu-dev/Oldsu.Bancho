using System.Data;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B394a;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared
{
    public struct SetPresence : ISharedPacket, Into<IB394APacketOut>
    {
        public User User { get; init; }
        public Stats Stats { get; init; }
        public Presence Presence { get; init; }
        public Status Status { get; init; }

        public IB394APacketOut Into()
        {
            HandleOsuUpdateOnlineUser packet;

            packet = new HandleOsuUpdateOnlineUser
            {
                UserID = (int)User.UserID,
                Username = User.Username,
                AvatarFilename = "ezchamp_old.jpg",
                Timezone = 0,
                Location = "Poopoo",
                RankedScore = (long)Stats.RankedScore,
                TotalScore = (long)Stats.TotalScore,
                Playcount = (int)Stats.Playcount,
                Accuracy = Stats.Accuracy / 100f,
                Rank = 0,
                BStatusUpdate = new bStatusUpdate
                {
                    bStatus = Status.bStatus,
                    BeatmapUpdate = true,
                    Map = Status.Map,
                    MapSha256 = Status.MapSha256,
                    Mods = Status.Mods,
                }
            };

            return packet;
        }
    }
}