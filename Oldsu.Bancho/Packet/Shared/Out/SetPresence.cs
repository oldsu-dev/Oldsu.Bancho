﻿namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct SetPresence : ISharedPacketOut, Into<IB394APacketOut>, Into<IB904PacketOut>
    {
        public ClientContext ClientInfo { get; init; }

        IB394APacketOut Into<IB394APacketOut>.Into()
        {
            Packet.Out.B394A.HandleOsuUpdateOnlineUser packet;

            if (ClientInfo.Stats != null)
            {
                packet = new()
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
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
            else
            {
                packet = new ()
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
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

            return packet;
        }
        
        IB904PacketOut Into<IB904PacketOut>.Into()
        {
            Packet.Out.B904.HandleOsuUpdateOnlineUser packet;

            if (ClientInfo.Stats != null)
            {
                packet = new()
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = (long)ClientInfo.Stats!.RankedScore,
                    TotalScore = (long)ClientInfo.Stats.TotalScore,
                    Playcount = (int)ClientInfo.Stats.Playcount,
                    Accuracy = ClientInfo.Stats.Accuracy / 100f,
                    Rank = 0,
                    Privileges = (byte)ClientInfo.Presence.Privilege,
                    BStatusUpdate = new Packet.Out.B904.bStatusUpdate
                    {
                        bStatus = ClientInfo.Activity!.Status,
                        BeatmapUpdate = new Packet.Out.B904.BeatmapUpdate
                        {
                            Map = ClientInfo.Activity.Map,
                            MapSha256 = ClientInfo.Activity.MapSHA256,
                            Mods = ClientInfo.Activity.Mods,
                            Gamemode = ClientInfo.Activity.GameMode,
                            MapId = 0,
                        }
                    }
                };
            }
            else
            {
                packet = new ()
                {
                    UserID = (int)ClientInfo.User!.UserID,
                    Username = ClientInfo.User.Username,
                    AvatarFilename = "old.jpg",
                    Timezone = 0,
                    Location = "Poopoo",
                    RankedScore = 0,
                    TotalScore = 0,
                    Playcount = 0,
                    Accuracy = 0 / 100f,
                    Rank = 0,
                    Privileges = (byte)ClientInfo.Presence.Privilege,
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