using System.Threading.Tasks;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public enum UserRequestTypes
    {
        QuitMatch,
        SubscribeToMatchSetup,
        AnnounceTransferHost
    }
    
    public interface IUserRequestObservable : IAsyncObservable<ProviderEvent> 
    {}

    public interface IUserRequestProvider
    { 
        Task RegisterUser(uint userId);
        Task UnregisterUser(uint userId);
        Task QuitMatch(uint userId);
        Task AnnounceTransferHost(uint userId);
        Task<IUserRequestObservable> GetObservable(uint userId);
    }
}