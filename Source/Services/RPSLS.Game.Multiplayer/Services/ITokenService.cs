using RPSLS.Game.Multiplayer.Models;
using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public interface ITokenService
    {
        Task<string> CreateToken(string username);

        Task JoinToken(string username, string token);

        Task<MatchResult> GetMatch(string username, string ticketId = null);
    }
}
