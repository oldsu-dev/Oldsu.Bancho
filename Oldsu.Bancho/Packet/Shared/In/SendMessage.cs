using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendMessage : ISharedPacketIn
    {
        public string Contents { get; init; }
        public string Target { get; init; }
        
        public async Task Handle(OnlineUser self)
        {
            await self.ServerMediator.Users.ReadAsync(users =>
            {
                users.BroadcastPacketToOthers(new BanchoPacket(new Out.SendMessage
                {
                    Sender = self.UserInfo.Username,
                    Contents = Contents,
                    Target = Target
                }), self.UserInfo.UserID);
            });
        }
    }
}