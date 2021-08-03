using System;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface ISpectatorObservable : IAsyncObservable<ProviderEvent> { }
    
    public interface IStreamerObservable : IAsyncObservable<ProviderEvent> { }


    public interface IStreamingProvider
    {
        Task<IStreamerObservable> GetStreamerObserver(uint userId);
        Task<ISpectatorObservable> GetSpectatorObserver(uint userId);
        Task PushFrames(uint userId, byte[] frameData);
        Task NotifySpectatorJoined(uint userId, uint spectatorUserId);
        Task NotifySpectatorLeft(uint spectatorUserId);
        Task RegisterStreamer(uint userId);
        Task UnregisterStreamer(uint userId);
    }
}