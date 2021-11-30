namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class FrameBundle : SharedPacketOut, IntoPacket<IGenericPacketOut>
    {
        public byte[] Frames { get; init; }

        public IGenericPacketOut IntoPacket()
        {
            var packet = new Packet.Out.Generic.SendFrameBundle
            {
                Data = Frames
            };

            return packet;
        }
    }
}