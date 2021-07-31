using System.Threading.Tasks;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class FrameBundle : ISharedPacketIn
    {
        public byte[] Frames { get; init; }
        
        public async Task Handle(OnlineUser self) =>
            await self.BroadcastPacketToSpectators(new BanchoPacket(new Out.FrameBundle {Frames = Frames}));
    }
}