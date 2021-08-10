using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.Generic
{
    [BanchoPacket(79, Version.NotApplicable, BanchoPacketType.In)]
    public struct ChannelLeave : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable()] public string ChannelName;

        public ISharedPacketIn IntoPacket() => new Shared.In.ChannelLeave {ChannelName = ChannelName};
    }
}