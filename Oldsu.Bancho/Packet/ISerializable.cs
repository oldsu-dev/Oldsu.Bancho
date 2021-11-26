using System;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Packet
{
    public interface ISerializable
    {
        ReadOnlyMemory<byte>? SerializeDataByVersion(Version version);
    }
}