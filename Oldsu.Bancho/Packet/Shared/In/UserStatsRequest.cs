using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            await client.SendPacket(new BanchoPacket(
                new StatusUpdate { Client = client, Completeness = Completeness.Self })
            ); 
        }
    }
}