using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(64, Version.NotApplicable, BanchoPacketType.In)]
    public class ChannelJoin : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable()] public string ChannelName;

        public ISharedPacketIn IntoPacket() => new Shared.In.ChannelJoin {ChannelName = ChannelName};
    }
}