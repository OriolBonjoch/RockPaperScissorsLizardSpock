using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IMultiplayerGameService : IGameService
    {
        string MatchId { get; set; }
        Task FetchMatchId(string username, Action<string, string, string> matchIdCallback);
        Task UserPick(string username, int pick);
        Task AddGameListener(string username, Action<ResultDto> gameListener);
    }
}
