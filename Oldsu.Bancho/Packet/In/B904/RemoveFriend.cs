using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904 {
    [BanchoPacket(75, Version.B904, BanchoPacketType.In)]
    public class RemoveFriend : IntoPacket<ISharedPacketIn> {
        [BanchoSerializable] public int UserID;
        public ISharedPacketIn IntoPacket() => new Shared.In.RemoveFriend(this.UserID);
    }
}
