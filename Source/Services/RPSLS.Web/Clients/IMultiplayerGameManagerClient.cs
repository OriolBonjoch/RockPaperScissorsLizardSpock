using RPSLS.Web.Models;
using System;
using System.Threading.Tasks;

namespace RPSLS.Web.Clients
{
    public interface IMultiplayerGameManagerClient
    {
        Task<string> CreatePairing(string username);

        Task JoinPairing(string username, string token);

        Task<MatchFoundDto> PairingStatus(string username, Action<string, string> matchIdCallback);
    }
}
