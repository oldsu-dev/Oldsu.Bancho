using System.Collections.Generic;
using System.Linq;
using Oldsu.Enums;
using Oldsu.Types;

namespace Oldsu.Bancho.Packet.Shared.Out {
    public class BanchoFriendsList : SharedPacketOut, IntoPacket<IB904PacketOut> {
        public List<Friendship> Friendships;
        
        public IB904PacketOut IntoPacket() {
            Packet.Out.B904.BanchoFriendsList friendsList = new();
            friendsList.FriendsList = new List<int>();

            foreach (Friendship friendship in this.Friendships) {
                friendsList.FriendsList.Add((int)friendship.FriendUserID);
            }

            return friendsList;
        }
    }
}
