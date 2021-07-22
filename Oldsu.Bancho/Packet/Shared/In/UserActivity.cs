using System.Threading.Tasks;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public byte Status;
        public string Map;
        public string MapSHA256;
        public ushort Mods;
        public byte GameMode;
        public int MapID;

        public async Task Handle(Client client)
        {
            await client.ClientContext!.WriteAsync(async context =>
            {
                context.Activity = this;
                await client.Server.BroadcastPacketAsync(new BanchoPacket( 
                    new StatusUpdate { ClientInfo = context, Completeness = Completeness.Online } )
                );
            });
        }
    }
}