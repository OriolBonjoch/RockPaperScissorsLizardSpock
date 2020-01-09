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

        public Task<string> GetToken(string username) => _gameManager.CreatePairing(username);

        public async Task WaitForMatchId(string username, Action<string, string> matchIdCallback)
        {
            var matchFound = await _gameManager.PairingStatus(username, true, matchIdCallback);
            MatchId = matchFound.MatchId;
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
    }
}
