using GameApi.Proto;
using RPSLS.Web.Clients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class BotGameService : GameService, IBotGameService
    {
        private ChallengerDto _challenger;

        public BotGameService(IGameManagerClient gameManager) : base(gameManager) { }

        public ChallengerDto Challenger
        {
            get => _challenger;
            set
            {
                _challenger = value;
                OpponentName = value?.DisplayName ?? "-";
            }
        }

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
