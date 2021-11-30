using Oldsu.Bancho.Packet;

namespace Oldsu.Bancho.GameLogic.Events
{
    public class HubEventPacket : HubEvent
    {
        public HubEventPacket(User invoker, ISharedPacketIn packet) : base(invoker)
        {
            Packet = packet;
        }
        
        public ISharedPacketIn Packet { get; }
        
        public override void Handle(HubEventContext context)
        {
            Packet.Handle(context);
        }
    }
}