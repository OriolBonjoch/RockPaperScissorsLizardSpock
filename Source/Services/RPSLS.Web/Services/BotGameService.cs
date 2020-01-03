using RPSLS.Web.Clients;
using RPSLS.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class BotGameService : GameService, IBotGameService
    {
        public BotGameService(IGameManagerClient gameManager) : base(gameManager) { }

        public async Task Play(string username, bool isTwitterUser)
        {
            GameResult = await GameManager.Play(
               Challenger.Name,
               username,
               Pick,
               isTwitterUser);
        }

        public Task<IEnumerable<ChallengerDto>> Challengers() => GameManager.Challengers();
    }
}
