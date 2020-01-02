using GameApi.Proto;
using RPSLS.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IMultiplayerGameService : IGameService
    {
        Task<string> GetToken(string username);
        Task WaitForMatchId(string username, Action<string, string> matchIdCallback);
    }

    public interface IGameService
    {
        int Pick { get; set; }
        public string OpponentName { get; }
        ResultDto GameResult { get; set; }
    }

    public interface IBotGameService : IGameService
    {
        ChallengerDto Challenger { get; set; }
        Task Play(string username, bool isTwitterUser);
        Task<IEnumerable<ChallengerDto>> Challengers();
    }
}
