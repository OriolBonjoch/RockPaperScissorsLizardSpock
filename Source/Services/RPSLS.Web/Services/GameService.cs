using GameApi.Proto;
using RPSLS.Web.Clients;
using RPSLS.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public abstract class GameService
    {
        public GameService(IGameManagerClient gameManager)
        {
            GameManager = gameManager;
        }

        public IGameManagerClient GameManager { get; private set; }
        public int Pick { get; set; }
        public string OpponentName { get; protected set; }
        public ResultDto GameResult { get; set; }
    }

    public class MultiplayerGameService : GameService, IMultiplayerGameService
    {
        private readonly ITokenManagerClient _tokenManager;

        public MultiplayerGameService(IGameManagerClient gameManager, ITokenManagerClient tokenManager) : base(gameManager)
        {
            _tokenManager = tokenManager;
        }

        public Task<string> GetToken(string username) => _tokenManager.CreateToken(username);

        public Task WaitForMatchId(string username, Action<string, string> matchIdCallback) => _tokenManager.WaitMatch(username, matchIdCallback);
    }

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
