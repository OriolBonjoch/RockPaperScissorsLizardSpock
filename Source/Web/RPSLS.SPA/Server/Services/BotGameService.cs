using RPSLS.SPA.Server.Clients;
using RPSLS.SPA.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Services
{
    public class BotGameService : GameService, IBotGameService
    {
        private readonly IBotGameManagerClient _gameManager;

        public BotGameService(IBotGameManagerClient gameManager)
        {
            _gameManager = gameManager;
        }

        public async Task Play()
        {
            GameResult = await _gameManager.Play(
               Challenger.Name,
               Username,
               Pick,
               IsTwitterUser);
        }

        public Task<IEnumerable<ChallengerDto>> Challengers() => _gameManager.Challengers();

        public void SetChallenger(ChallengerDto challenger)
        {
            Challenger = challenger;
        }
    }
}
