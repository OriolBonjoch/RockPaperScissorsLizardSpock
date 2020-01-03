using RPSLS.Web.Clients;
using RPSLS.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class BotGameService : GameService, IBotGameService
    {
        private readonly IBotGameManagerClient _gameManager;

        public BotGameService(IBotGameManagerClient gameManager) {
            _gameManager = gameManager;
        }

        public async Task Play(string username, bool isTwitterUser)
        {
            GameResult = await _gameManager.Play(
               Challenger.Name,
               username,
               Pick,
               isTwitterUser);
        }

        public Task<IEnumerable<ChallengerDto>> Challengers() => _gameManager.Challengers();
    }
}
