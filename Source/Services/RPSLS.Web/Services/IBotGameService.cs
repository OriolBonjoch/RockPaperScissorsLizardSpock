using GameApi.Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IBotGameService : IGameService
    {
        ChallengerDto Challenger { get; set; }
        Task Play(string username, bool isTwitterUser);
        Task<IEnumerable<ChallengerDto>> Challengers();
    }
}
