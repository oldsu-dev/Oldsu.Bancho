using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Logging;

namespace Oldsu.Bancho.Packet
{

    
    public interface ISharedPacketIn
    {
        public void Handle(HubEventContext context);
    }
}