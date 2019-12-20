using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public interface IPlayFabService
    {
        Task Initialize();
        Task<string> CreateTicket(string username, string token = "random");
        Task<string> CheckTicketStatus(string username);
    }
}
