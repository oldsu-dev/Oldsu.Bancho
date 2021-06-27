using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class UserActivity : ISharedPacketIn
    {
        public byte Status;
        public string Map;
        public string MapSHA256;
        public ushort Mods;
        public byte Gamemode;
        public int MapID;

        public async Task Handle(Client client)
        {
            client.Activity = this;
            
            Client.BroadcastPacket(new BanchoPacket( 
                new StatusUpdate { Client = client, Completeness = Completeness.Online } )
            );
        }
    }
}