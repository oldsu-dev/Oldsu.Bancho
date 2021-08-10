using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class FrameBundle : ISharedPacketIn
    {
        public byte[] Frames { get; init; }

        public Task Handle(UserContext userContext, Connection _) =>
            userContext.StreamingProvider.PushFrames(userContext.UserID, Frames);
    }
}