using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public interface ITokenService
    {
        Task<string> CreateToken(string username);

        Task JoinToken(string username, string token);

        Task<string> GetMatch(string username);
    }
}
