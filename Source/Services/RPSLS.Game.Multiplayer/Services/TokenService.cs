using Microsoft.Extensions.Options;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public class TokenService : ITokenService
    {
        private readonly IPlayFabService _playFabService;
        private readonly TokenSettings _settings;

        public TokenService(IPlayFabService playFabService, IOptions<TokenSettings> options)
        {
            _playFabService = playFabService;
            _settings = options.Value;
        }

        public async Task<string> CreateToken(string username)
        {
            var token = GenerateToken();
            await _playFabService.CreateTicket(username, token);
            return token;
        }

        public Task<MatchResult> GetMatch(string username, string ticketId = null) => _playFabService.CheckTicketStatus(username, ticketId);

        public Task JoinToken(string username, string token) => _playFabService.CreateTicket(username, token);

        private string GenerateToken()
        {
            var random = new Random();
            return new string(Enumerable.Repeat(_settings.ValidCharacters, _settings.Length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
