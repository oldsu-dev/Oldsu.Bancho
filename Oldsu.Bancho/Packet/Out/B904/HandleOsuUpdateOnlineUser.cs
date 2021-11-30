using Oldsu.Bancho.Objects;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904
{
    [BanchoPacket(12, Version.B904, BanchoPacketType.Out)]
    public class HandleOsuUpdateOnlineUser : IB904PacketOut
    {
        [BanchoSerializable] public int UserID;
        [BanchoSerializable] public byte Completeness = 2;
        [BanchoSerializable] public bStatusUpdate BStatusUpdate;
        [BanchoSerializable] public long RankedScore;
        [BanchoSerializable] public float Accuracy;
        [BanchoSerializable] public int Playcount;
        [BanchoSerializable] public long TotalScore;
        [BanchoSerializable] public uint Rank;
        [BanchoSerializable] public string Username;
        [BanchoSerializable] public string AvatarFilename;
        [BanchoSerializable] public byte Timezone;
        [BanchoSerializable] public string Location;
        [BanchoSerializable] public byte Privileges;
    }
}