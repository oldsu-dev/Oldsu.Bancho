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

    public class UserRequest
    {
        public UserRequestTypes Type { get; set; }
        public TaskCompletionSource? RequestFulfiller { get; set; }
    }
    
    public interface IUserRequestObservable : IAsyncObservable<ProviderEvent> 
    {}

    public interface IUserRequestProvider
    { 
        Task RegisterUser(uint userId);
        Task UnregisterUser(uint userId);
        Task<Task> QuitMatch(uint userId);
        Task<Task> AnnounceTransferHost(uint userId);
        Task<Task> SubscribeToMatchUpdates(uint userId);
        Task<IUserRequestObservable> GetObservable(uint userId);
    }
}