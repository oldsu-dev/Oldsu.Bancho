using Oldsu.Bancho.Objects;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B394a
{
    [BanchoPacket(12, Version.B394A, BanchoPacketType.Out)]
    public class HandleOsuUpdateSelf : IB394APacketOut
    {
        [BanchoSerializable] public int UserID;
        [BanchoSerializable] public byte Completeness = 1;
        [BanchoSerializable] public bStatusUpdate BStatusUpdate;
        [BanchoSerializable] public long RankedScore;
        [BanchoSerializable] public float Accuracy;
        [BanchoSerializable] public int Playcount;
        [BanchoSerializable] public long TotalScore;
        [BanchoSerializable] public ushort Rank;
    }
}