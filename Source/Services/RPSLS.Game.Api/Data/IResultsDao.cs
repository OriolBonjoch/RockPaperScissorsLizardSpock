using RPSLS.Game.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Game.Api.Data
{
    public interface IResultsDao
    {
        Task<MatchDto> GetMatch(string matchId);
        Task CreateMatch(string matchId, string username, string challenger);
        Task<MatchDto> SaveMatchPick(string matchId, string username, int pick);
        Task<MatchDto> SaveMatchResult(string matchId, GameApi.Proto.Result result);
        Task SaveMatch(PickDto pick, string username, int userPick, GameApi.Proto.Result result);
        Task DeleteMatch(string matchId);
        Task<IEnumerable<MatchDto>> GetLastGamesOfPlayer(string player, int limit);
    }
}
