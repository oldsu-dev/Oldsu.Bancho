using Oldsu.Bancho.Exceptions.ChatChannel;
using Oldsu.Bancho.Exceptions.PacketHandling;
using Oldsu.Bancho.GameLogic;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class SendPrivateMessage : ISharedPacketIn
    {
        public string? Contents { get; init; }
        public string? Target { get; init; }

        public void Handle(HubEventContext context)
        {
            if (Contents == null || Target == null)
                throw new NullStringReceivedException();            
            
            if (context.Hub.UserPanelManager.EntitiesByUsername.TryGetValue(Target, out var entity))
                entity.User.SendPrivateMessage(context.User!, Contents);
            else
                throw new InvalidChannelException();
        }
    }
}