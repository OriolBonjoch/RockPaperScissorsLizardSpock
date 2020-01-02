using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Clients
{
    public interface ITokenManagerClient
    {
        Task<string> CreateToken(string username);

        Task Join(string username, string token);

        Task<string> WaitMatch(string username, Action<string, string> matchIdCallback);
    }
}
