using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StopSpectating : ISharedPacketIn
    {
        public void Handle(HubEventContext context) =>
            context.Hub.UserPanelManager.StopSpectating(context.User!);
    }
}