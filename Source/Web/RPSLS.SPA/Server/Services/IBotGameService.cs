using RPSLS.SPA.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Services
{
    public interface IBotGameService : IGameService
    {
        Task Play();
        Task<IEnumerable<ChallengerDto>> Challengers();
        void SetChallenger(ChallengerDto challenger);
    }
}
