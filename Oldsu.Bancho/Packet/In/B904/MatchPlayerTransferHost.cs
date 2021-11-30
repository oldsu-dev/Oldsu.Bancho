using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(71, Version.B904, BanchoPacketType.In)]
    public struct MatchPlayerTransferHost : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable()]
        public uint SlotID;

        public ISharedPacketIn IntoPacket() => new Shared.In.MatchPlayerTransferHost {SlotID = SlotID};
    }
}