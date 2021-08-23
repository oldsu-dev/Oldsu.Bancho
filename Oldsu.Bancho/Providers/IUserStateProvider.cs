using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IUserStateProvider : IAsyncObservable<ProviderEvent>
    {
        Task RegisterUserAsync(uint userId, UserData data);
        Task UnregisterUserAsync(uint userId);
        Task<IEnumerable<UserData>> GetAllUsersAsync();
        Task SetActivityAsync(uint userId, Activity activity);
        Task SetStatsAsync(uint userId, StatsWithRank? stats);
        Task<bool> IsUserOnline(uint userId);
    }
}