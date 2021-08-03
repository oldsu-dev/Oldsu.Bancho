using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StartSpectating : ISharedPacketIn
    {
        public int UserID { get; set; }
        
        public async Task Handle(UserContext userContext, Connection _)
        {
            await userContext.StreamingProvider.NotifySpectatorJoined(
                (uint)UserID, (uint)userContext.UserID);

            await userContext.SubscriptionManager.SubscribeToSpectatorObservable(
                await userContext.StreamingProvider.GetSpectatorObserver((uint)UserID));
        }
    }
}