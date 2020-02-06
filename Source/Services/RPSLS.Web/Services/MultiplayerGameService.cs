using RPSLS.Web.Clients;
using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class MultiplayerGameService : GameService, IMultiplayerGameService
    {
        private readonly IMultiplayerGameManagerClient _gameManager;

        public MultiplayerGameService(IMultiplayerGameManagerClient gameManager)
        {
            _gameManager = gameManager;
        }

        public string MatchId { get; set; }

        public async Task FetchMatchId(string username, Action<string, string, string> matchIdCallback)
        {
            MatchId = await _gameManager.CreatePairing(username, matchIdCallback);
        }

        public async Task AddGameListener(string username, Action<ResultDto> gameListener)
        {
            await _gameManager.GameStatus(MatchId, username, gameListener);
        }

        public async Task UserPick(string username, int pick)
        {
            Pick = pick;
            await _gameManager.Pick(MatchId, username, pick);
        }

        public async Task<bool> Rematch(string username) => await _gameManager.Rematch(MatchId, username);
    }
}
