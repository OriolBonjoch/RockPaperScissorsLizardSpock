using GameApi.Proto;
using RPSLS.Web.Clients;
using RPSLS.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public class GameService : IGameService
    {
        private readonly IGameManagerClient _gameManager;
        private readonly ITokenManagerClient _tokenManager;
        public GameService(IGameManagerClient gameManager, ITokenManagerClient tokenManager)
        {
            _gameManager = gameManager;
            _tokenManager = tokenManager;
        }

        public ChallengerDto Challenger { get; set; }
        public int Pick { get; set; }
        public ResultDto GameResult { get; set; }

        public async Task Play(string username, bool isTwitterUser)
        {
            GameResult = await _gameManager.Play(
               Challenger.Name,
               username,
               Pick,
               isTwitterUser);
        }

        public Task<IEnumerable<ChallengerDto>> Challengers()
        {
            return _gameManager.Challengers();
        }

        public Task<string> GetToken(string username) => _tokenManager.CreateToken(username);

        public Task<bool> CheckToken(string username) => _tokenManager.Matched(username);
    }
}
