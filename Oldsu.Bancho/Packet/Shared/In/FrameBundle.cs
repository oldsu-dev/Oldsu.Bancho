using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class FrameBundle : ISharedPacketIn
    {
        public byte[] Frames { get; init; }

        public void Handle(HubEventContext context)
        {
            context.Hub.UserPanelManager.EntitiesByUserID[context.User!.UserID]
                .BroadcastToSpectators(new Out.FrameBundle{Frames = Frames});
        }
    }
}