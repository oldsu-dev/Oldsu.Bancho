using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Out.B394a;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct StatusUpdate : ISharedPacketOut, Into<IB394APacketOut>
    {
        public Client Client { get; init; }
        public Completeness Completeness { get; init; }

        public IB394APacketOut Into()
        {
            dynamic packet;
            if (Completeness == Completeness.Self)
            {
                if (Client.Stats != null) 
                {
                    packet = new HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = (long)Client.Stats.RankedScore,
                        TotalScore = (long)Client.Stats.TotalScore,
                        Playcount = (int)Client.Stats.Playcount,
                        Accuracy = (Client.Stats.Accuracy / 100f),
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
                }
                else 
                {
                    packet = new HandleOsuUpdateSelf
                    {
                        UserID = (int)Client.User!.UserID,
                        RankedScore = 0,
                        TotalScore = 0,
                        Playcount = 0,
                        Accuracy = 0 / 100f,
                        Rank = 0,
                        BStatusUpdate = new bStatusUpdate
                        {
                            bStatus = Client.Activity!.Status,
                            BeatmapUpdate = new BeatmapUpdate
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
                packet = new HandleOsuUpdateOnlineUser
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
            }
            
            return packet;
        }
    }
}