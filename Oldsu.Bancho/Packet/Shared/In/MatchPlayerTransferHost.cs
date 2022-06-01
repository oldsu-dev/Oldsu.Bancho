using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.Lobby;
using Oldsu.Bancho.GameLogic;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchPlayerTransferHost : ISharedPacketIn
    {
        public uint SlotID { get; set; }

        public void Handle(HubEventContext context)
        {
            if (context.User!.Match == null)
                throw new UserNotInMatchException();
            
            context.User.Match.TransferHost(context.User, SlotID);
        }
    }
}