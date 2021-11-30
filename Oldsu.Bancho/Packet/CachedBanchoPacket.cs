using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Oldsu.Bancho.Packet;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class CachedBanchoPacket : ISerializable
    {
        private readonly ConcurrentDictionary<Version, ReadOnlyMemory<byte>?> _cachedData = new();
        private readonly SharedPacketOut? _payload;

        public CachedBanchoPacket(SharedPacketOut payload)
        {
            _payload = payload;
        }

        public ReadOnlyMemory<byte>? SerializeDataByVersion(Version version)
        {
            if (_payload == null)
                return null;
            
            return _cachedData.GetOrAdd(version, _payload.SerializeDataByVersion);
        }
    }
}