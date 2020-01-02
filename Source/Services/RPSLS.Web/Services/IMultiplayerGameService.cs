using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IMultiplayerGameService : IGameService
    {
        string MatchId { get; set; }
        Task<string> GetToken(string username);
        Task WaitForMatchId(string username, Action<string, string> matchIdCallback);
    }
}
