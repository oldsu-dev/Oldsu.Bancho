using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class FrameBundle : ISharedPacketIn
    {
        public byte[] Frames { get; init; }
        
        public async Task Handle(Client client)
        {
            client.ClientContext?.SpectatorContext.BroadcastFrames( new Out.FrameBundle
            {
                Frames = Frames
            });
        }
    }
}