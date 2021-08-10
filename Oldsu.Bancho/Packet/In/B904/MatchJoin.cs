using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(33, Version.B904, BanchoPacketType.In)]
    public class MatchJoin : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable]
        public uint MatchID;
        
        [BanchoSerializable]
        public string Password;

        public ISharedPacketIn IntoPacket() => new Shared.In.MatchJoin {GamePassword = Password, MatchID = MatchID};
    }
}