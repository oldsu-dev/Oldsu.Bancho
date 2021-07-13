using Oldsu.Bancho.Objects;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B394A
{
    [BanchoPacket(12, Version.B394A, BanchoPacketType.Out)]
    public class HandleOsuUpdateOnlineUser : IB394APacketOut
    {
        [BanchoSerializable] public int UserID;
        [BanchoSerializable] public byte Completeness = 2;
        [BanchoSerializable] public bStatusUpdate BStatusUpdate;
        [BanchoSerializable] public long RankedScore;
        [BanchoSerializable] public float Accuracy;
        [BanchoSerializable] public int Playcount;
        [BanchoSerializable] public long TotalScore;
        [BanchoSerializable] public ushort Rank;
        [BanchoSerializable] public string Username;
        [BanchoSerializable] public string AvatarFilename;
        [BanchoSerializable] public byte Timezone;
        [BanchoSerializable] public string Location;
    }
}