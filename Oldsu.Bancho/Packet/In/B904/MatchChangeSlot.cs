using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(39, Version.B904, BanchoPacketType.In)]
    public struct MatchChangeSlot : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable] public int SlotID;
        
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchChangeSlot{SlotID = SlotID};
    }
}