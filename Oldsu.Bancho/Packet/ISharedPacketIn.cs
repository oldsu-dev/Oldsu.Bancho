using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet
{
    public interface ISharedPacketIn
    {
        public Task Handle(UserContext userContext, Connection connection);
    }
}