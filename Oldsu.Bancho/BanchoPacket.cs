using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Oldsu.Bancho.Packet;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public class BanchoPacket
    {
        private readonly ConcurrentDictionary<Version, byte[]> _cachedData = new();
        private readonly ISharedPacketOut _payload;

        public BanchoPacket(ISharedPacketOut payload)
        {
            _payload = payload;
        }

        private byte[] SerializeDataByVersion(Version version)
        {
            object? packet;

            if (_payload is Into<IGenericPacketOut> generic)
                packet = generic.Into();
            else
            {
                packet = version switch
                {
                    Version.B394A => (_payload as Into<IB394APacketOut>)?.Into(),
                    Version.B904 => (_payload as Into<IB904PacketOut>)?.Into(),
                    
                    Version.NotApplicable =>
                        throw new InvalidOperationException("This version is not applicable"),
                
                    _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
                };
            }

            return packet == null ? Array.Empty<byte>() : BanchoSerializer.Serialize(packet);
        }

        public byte[] GetDataByVersion(Version version, bool cache = true) =>
            !cache ? SerializeDataByVersion(version) : _cachedData.GetOrAdd(version, SerializeDataByVersion);
    }
}