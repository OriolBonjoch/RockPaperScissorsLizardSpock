using RPSLS.Web.Clients;
using RPSLS.Web.Models;

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
}
