using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IMultiplayerGameService : IGameService
    {
        string MatchId { get; set; }
        Task FetchMatchId(string username, bool isTwitter, Action<string, string, string> matchIdCallback);
        Task FetchMatchId(string username, bool isTwitter, string token);
        Task UserPick(string username, int pick);
        Task AddGameListener(string username, Action<ResultDto> gameListener);
        Task<bool> Rematch(string username);
    }
}
