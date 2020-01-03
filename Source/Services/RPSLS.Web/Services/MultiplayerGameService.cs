using RPSLS.Web.Clients;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class MultiplayerGameService : GameService, IMultiplayerGameService
    {
        private readonly ITokenManagerClient _tokenManager;

        public MultiplayerGameService(IGameManagerClient gameManager, ITokenManagerClient tokenManager) : base(gameManager)
        {
            _tokenManager = tokenManager;
        }

        public string MatchId { get; set; }

        public Task<string> GetToken(string username) => _tokenManager.CreateToken(username);

        public async Task WaitForMatchId(string username, Action<string, string> matchIdCallback)
        {
            var matchFound = await _tokenManager.WaitMatch(username, matchIdCallback);
            MatchId = matchFound.MatchId;
        }
    }
}
