using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class FrameBundle : ISharedPacketIn
    {
        public byte[] Frames { get; init; }
        
        public async Task Handle(Client client)
        {
            client.BroadcastFramesAsync( new Out.FrameBundle
            {
                Frames = Frames
            });
        }
    }
}