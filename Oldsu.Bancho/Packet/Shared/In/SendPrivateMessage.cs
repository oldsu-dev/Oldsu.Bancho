using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendPrivateMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public async Task Handle(UserContext self, Connection conn)
        {
            // await self.ServerMediator.Users.ReadAsync(users =>
            // {
            //     users.SendPacketToSpecificUser(new BanchoPacket(new Out.SendMessage
            //     {
            //         Sender = self.UserInfo.Username,
            //         Contents = Contents,
            //         Target = Target
            //     }), Target);
            // });
        }
    }
}