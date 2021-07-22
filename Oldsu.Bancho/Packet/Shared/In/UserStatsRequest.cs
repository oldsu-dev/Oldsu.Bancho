using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserStatsRequest : ISharedPacketIn
    {
        public async Task Handle(Client client)
        {
            await client.ClientContext!.ReadAsync(async context =>
            {
                await client.SendPacketAsync(new BanchoPacket(
                    new StatusUpdate { ClientInfo = context, Completeness = Completeness.Self })
                );
            });

        }
    }
}