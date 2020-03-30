using RPSLS.SPA.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPSLS.SPA.Server.Clients
{
    public interface IBotGameManagerClient
    {
        Task<ResultDto> Play(string challenger, string username, int pick, bool twitterLogged);

        Task<IEnumerable<ChallengerDto>> Challengers();
    }
}
