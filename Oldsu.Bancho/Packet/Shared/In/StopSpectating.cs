using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class StopSpectating : ISharedPacketIn
    {
        public async Task Handle(UserContext userContext, Connection _)
        {
            await userContext.Dependencies.Get<IStreamingProvider>().NotifySpectatorLeft(userContext.UserID);
            await userContext.SubscriptionManager.UnsubscribeFromSpectatorObservable();
        }
    }
}