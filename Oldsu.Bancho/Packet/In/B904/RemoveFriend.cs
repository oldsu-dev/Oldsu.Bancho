namespace Oldsu.Bancho.Packet.In.B904 {
    public class RemoveFriend : IntoPacket<ISharedPacketIn> {
        [BanchoSerializable] public int UserID;
        public ISharedPacketIn IntoPacket() => new Shared.In.RemoveFriend(this.UserID);
    }
}
