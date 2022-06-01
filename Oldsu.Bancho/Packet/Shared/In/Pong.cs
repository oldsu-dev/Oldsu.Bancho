using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class Pong : ISharedPacketIn
    {
        public void Handle(HubEventContext context)
        {
            context.User!.SendPacket(new Ping());
        }
    }
}