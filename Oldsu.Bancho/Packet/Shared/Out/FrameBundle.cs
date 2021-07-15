namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class FrameBundle : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public byte[] Frames { get; init; }

        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.SendFrameBundle
            {
                Frames = Frames
            };

            return packet;
        }
    }
}