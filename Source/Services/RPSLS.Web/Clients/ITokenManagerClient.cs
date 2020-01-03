using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Clients
{
    public interface ITokenManagerClient
    {
        Task<string> CreateToken(string username);

        Task Join(string username, string token);

        Task<MatchFoundDto> WaitMatch(string username, Action<string, string> matchIdCallback);
    }
}
