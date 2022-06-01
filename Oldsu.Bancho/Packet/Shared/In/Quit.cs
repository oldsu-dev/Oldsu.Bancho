using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Quit : ISharedPacketIn
    {
        public void Handle(HubEventContext context)
        {
            context.HubEventLoop.SendEvent(new HubEventDisconnect(context.User!));
        }
    }
}