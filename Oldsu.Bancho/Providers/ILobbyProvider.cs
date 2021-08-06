using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Objects;
using Oldsu.Utils;

namespace Oldsu.Bancho.Providers
{
    public interface IMatchObservable : IAsyncObservable<ProviderEvent> { }
    
    public interface ILobbyProvider : IAsyncObservable<ProviderEvent>
    {
        Task<bool> MatchMoveSlot(uint userId, int newSlot);
        Task<bool> MatchNoBeatmap(uint userId);
        Task<bool> MatchGotBeatmap(uint userId);
        Task<bool> MatchSetUnready(uint userId);
        Task<bool> MatchSetReady(uint userId);
        Task<uint?> GetCurrentMatch(uint userId);
        Task<bool> MatchChangeSettings(uint userId, MatchSettings matchSettings);
        Task<bool> MatchChangeMods(uint userId, short mods);
        Task<IMatchObservable?> MatchGetObservable(uint id);
        Task<MatchState[]> GetAvailableMatches();
        Task<MatchState?> JoinMatch(uint userId, uint matchId, string password);
        Task<MatchState?> GetMatchState(uint matchId);
        Task<MatchState?> CreateMatch(uint userId, MatchSettings match);
        Task<bool> LeaveMatch(uint userId);
        Task<bool> MatchLockSlot(uint userId, uint slot);
        Task<bool> MatchStart(uint userId);
        Task<bool> MatchScoreUpdate(uint userId, ScoreFrame scoreFrame);
        Task<bool> MatchSkip(uint userId);
        Task<bool> MatchLoad(uint userId);
        Task<bool> MatchComplete(uint userId);
    }
}