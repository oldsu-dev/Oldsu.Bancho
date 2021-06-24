using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Oldsu.Bancho.Packet;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public enum BanchoPacketType
    {
        In,
        Out
    }
    
    public class BanchoPacketAttribute : System.Attribute
    {
        public ushort Id { get; }

        public Version Version { get; }
        
        public BanchoPacketType Type { get; }
        
        public BanchoPacketAttribute(ushort id, Version version, BanchoPacketType type)
        {
            Id = id;
            Version = version;
            Type = type;
        }
    }

    public class BanchoPacket
    {
        private readonly ConcurrentDictionary<Version, byte[]> _cachedData = new();
        private readonly ISharedPacket _payload;

        public BanchoPacket(ISharedPacket payload)
        {
            _payload = payload;
        }

        private byte[] GetDataByVersion(Version version)
        {
            if (_payload == null)
                return null;

            object packet = version switch
            {
                Version.B394A => 
                    ((Into<IB394APacketOut>)_payload).Into(),
                
                Version.NotApplicable => 
                    throw new InvalidOperationException("This version is not applicable"),
                
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
            };

            return BanchoSerializer.Serialize(packet);
        }

        public byte[] GetDataByVersion(Oldsu.Enums.Version version, bool cache = true) =>
            !cache ? GetDataByVersion(version) : _cachedData.GetOrAdd(version, GetDataByVersion);
    }
}