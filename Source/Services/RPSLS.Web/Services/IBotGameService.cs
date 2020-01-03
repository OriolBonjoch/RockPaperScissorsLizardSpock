using RPSLS.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.Web.Services
{
    public interface IBotGameService : IGameService
    {
        Task Play(string username, bool isTwitterUser);
        Task<IEnumerable<ChallengerDto>> Challengers();
    }
}
