using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public Activity Activity { get; set; }

        public void Handle(HubEventContext context)
        {
            context.Hub.UserPanelManager.UpdateActivity(context.User!, Activity);
        }
    }
}