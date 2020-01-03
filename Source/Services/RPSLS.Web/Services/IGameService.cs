using RPSLS.Web.Models;

namespace RPSLS.Web.Services
{
    public interface IGameService
    {
        int Pick { get; set; }
        ChallengerDto Challenger { get; set; }
        ResultDto GameResult { get; set; }
    }
}
