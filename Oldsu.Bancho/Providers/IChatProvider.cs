using System.Threading.Tasks;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IChatChannel : IAsyncObservable<ProviderEvent>
    {
        Channel ChannelInfo { get; }
        public Task SendMessage(string username, string content);
    }

    public interface IChatProvider : IAsyncObservable<ProviderEvent>
    {
        Task RegisterUser(string username);
        Task RegisterChannel(Channel channel);
        Task UnregisterChannel(string channelTag);
        Task<Channel[]> GetAvailableChannelInfo(Privileges privileges);
        Task<Channel[]> GetAutojoinChannelInfo(Privileges privileges);
        Task<IChatChannel?> GetUserChannel(string username);
        Task<IChatChannel?> GetChannel(string tag, Privileges privileges);
        Task UnregisterUser(string username);
    }
}