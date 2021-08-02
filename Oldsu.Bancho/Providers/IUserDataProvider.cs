using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IUserDataProvider : IAsyncObservable<BanchoPacket>
    {
        Task RegisterUserAsync(uint userId, UserData data);
        Task UnregisterUserAsync(uint userId);
        Task<IEnumerable<UserData>> GetAllUsersAsync();
        Task SetActivityAsync(uint userId, Activity activity);
        Task SetStatsAsync(uint userId, StatsWithRank? stats);
        Task<UserData> GetUserAsync(uint userId);
        Task<Privileges> GetUserPrivilegesAsync(uint userId);
    }
}