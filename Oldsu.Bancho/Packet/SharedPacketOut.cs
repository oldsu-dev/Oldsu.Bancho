using System;
using System.Threading.Tasks;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Packet
{
    public class SharedPacketOut : ISerializable
    {
        public ReadOnlyMemory<byte>? SerializeDataByVersion(Version version)
        {
            object? packet;

            if (this is IntoPacket<IGenericPacketOut> generic)
                packet = generic.IntoPacket();
            else
            {
                packet = version switch
                {
                    Version.B904 => (this as IntoPacket<IB904PacketOut>)?.IntoPacket(),
                    
                    Version.NotApplicable =>
                        throw new InvalidOperationException("This version is not applicable"),
                
                    _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
                };
            }

            return packet == null ? Array.Empty<byte>() : BanchoSerializer.Serialize(packet);
        }
    }
}