namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class Signature : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public string ServerName { get; set; }
        public string Version { get; set; }

        public IGenericPacketOut IntoPacket() => new Packet.Out.Generic.Signature
        {
            Version = Version, 
            ServerName = ServerName
        };
    }
}