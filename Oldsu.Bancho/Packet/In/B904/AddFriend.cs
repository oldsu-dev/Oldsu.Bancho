using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904 {
    [BanchoPacket(74, Version.NotApplicable, BanchoPacketType.In)]
    public struct AddFriend : IntoPacket<ISharedPacketIn> {
        [BanchoSerializable] public int UserID;
        public ISharedPacketIn IntoPacket() => new Shared.In.AddFriend(this.UserID);
    }
}
