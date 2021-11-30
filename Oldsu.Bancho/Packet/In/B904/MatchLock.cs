using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(41, Version.B904, BanchoPacketType.In)]
    public class MatchLock : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable]
        public uint SlotID;
        
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchLock {SlotID = SlotID};
    }
}