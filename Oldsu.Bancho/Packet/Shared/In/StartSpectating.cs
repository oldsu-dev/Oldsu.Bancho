using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StartSpectating : ISharedPacketIn
    {
        public int UserID { get; set; }
        
        public async Task Handle(UserContext userContext, Connection _)
        {
            var streamingProvider = userContext.Dependencies.Get<IStreamingProvider>();
            
            await userContext.Dependencies.Get<IStreamingProvider>().NotifySpectatorJoined(
                (uint)UserID, (uint)userContext.UserID);

            await userContext.SubscriptionManager.SubscribeToSpectatorObservable(
                (await streamingProvider.GetSpectatorObserver((uint)UserID))!);
        }
    }
}