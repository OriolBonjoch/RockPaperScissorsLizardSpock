using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Clients
{
    public interface IMultiplayerGameManagerClient
    {
        Task<string> CreatePairing(string username, Action<string, string, string> matchIdCallback);

        Task<string> JoinPairing(string username, string token, Action<string, string, string> matchIdCallback);

        Task Pick(string matchId, string username, int pick);

        Task<ResultDto> GameStatus(string matchId, string username, Action<ResultDto> gameListener);

        Task<bool> Rematch(string matchId, string username);
    }
}
