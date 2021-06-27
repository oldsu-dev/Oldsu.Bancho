using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet
{
    public interface ISharedPacketIn
    {
        public Task Handle(Client client);
    }
}