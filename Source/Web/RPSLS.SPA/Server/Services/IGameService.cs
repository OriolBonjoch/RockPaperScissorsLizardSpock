using RPSLS.SPA.Shared.Models;

namespace RPSLS.SPA.Server.Services
{
    public interface IGameService
    {
        string Username { get; set; }
        bool IsTwitterUser { get; set; }
        int Pick { get; set; }
        ChallengerDto Challenger { get; set; }
        ResultDto GameResult { get; set; }
    }
}
