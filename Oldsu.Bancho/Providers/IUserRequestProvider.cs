using System.Threading.Tasks;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public enum UserRequestTypes
    {
        QuitMatch,
        SubscribeToMatchSetup
    }
    
    public interface IUserRequestObservable : IAsyncObservable<ProviderEvent> 
    {}

    public interface IUserRequestProvider
    { 
        Task RegisterUser(uint userId);
        Task UnregisterUser(uint userId);
        Task QuitMatch(uint userId);
        Task<IUserRequestObservable> GetObservable(uint userId);
    }
}