using RPSLS.Web.Models;

namespace RPSLS.Web.Services
{
    public interface IGameService
    {
        int Pick { get; set; }
        public string OpponentName { get; }
        ResultDto GameResult { get; set; }
    }
}
