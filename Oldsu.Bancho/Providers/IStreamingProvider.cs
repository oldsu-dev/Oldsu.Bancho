using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.In;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IStreamingProvider
    {
        Task StartSpectating(uint userId, uint targetUserId);
        Task StopSpectating();
        Task PushFrames(FrameBundle frameBundle, uint userId);
    }
}