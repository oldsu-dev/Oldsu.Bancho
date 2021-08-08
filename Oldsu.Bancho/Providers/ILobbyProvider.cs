using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Objects;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IMatchSetupObservable : IAsyncObservable<ProviderEvent> { }
    public interface IMatchGameObservable : IAsyncObservable<ProviderEvent> { }
    
    public interface ILobbyProvider : IAsyncObservable<ProviderEvent>
    {
        Task MatchMoveSlot(uint userId, uint newSlot);
        Task MatchNoBeatmap(uint userId);
        Task MatchGotBeatmap(uint userId);
        Task MatchSetUnready(uint userId);
        Task MatchSetReady(uint userId);
        Task MatchChangeSettings(uint userId, MatchSettings matchSettings);
        Task MatchChangeMods(uint userId, short mods);
        Task<IMatchSetupObservable?> GetMatchSetupObservable(uint userId);
        Task<IMatchGameObservable?> GetMatchGameObservable(uint userId);
        Task<MatchState[]> GetAvailableMatches();
        Task<MatchState?> JoinMatch(uint userId, uint matchId, string password);
        Task<MatchState?> GetMatchState(uint matchId);
        Task<MatchState?> CreateMatch(uint userId, MatchSettings match);
        Task<bool> TryLeaveMatch(uint userId);
        Task<uint?> MatchLockSlot(uint userId, uint slot);
        Task MatchStart(uint userId);
        Task MatchScoreUpdate(uint userId, ScoreFrame scoreFrame);
        Task MatchSkip(uint userId);
        Task MatchLoad(uint userId);
        Task<bool> MatchComplete(uint userId);
        Task MatchChangeTeam(uint userContextUserId);
    }
}