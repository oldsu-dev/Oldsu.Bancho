using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.In.B904
{
    [BanchoPacket(52, Version.B904, BanchoPacketType.In)]
    public struct MatchChangeMods : IntoPacket<ISharedPacketIn>
    {
        [BanchoSerializable] public int Mods;
        
        public ISharedPacketIn IntoPacket() => new Shared.In.MatchChangeMods {Mods = Mods};
    }
}