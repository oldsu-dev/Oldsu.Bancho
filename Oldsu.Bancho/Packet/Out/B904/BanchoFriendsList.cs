using System.Collections.Generic;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Out.B904 {
    [BanchoPacket(73, Version.NotApplicable, BanchoPacketType.In)]
    public class BanchoFriendsList : IB904PacketOut {
        [BanchoSerializable] public List<int> FriendsList;
    }
}
