using Grpc.Core;
using Microsoft.Extensions.Logging;
using RPSLS.Game.Multiplayer.Services;
using System.Threading.Tasks;
using TokenApi.Proto;

namespace RPSLS.Game.Api.GrpcServices
{
    public class TokenManagerService : TokenManager.TokenManagerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenManagerService> _logger;

        public TokenManagerService(
            ITokenService tokenService,
            ILogger<TokenManagerService> logger
            )
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public override async Task<CreateTokenResponse> CreateToken(CreateTokenRequest request, ServerCallContext context)
        {
            var token = await _tokenService.CreateToken(request.Username);
            return new CreateTokenResponse() { Token = token };
        }

        public override async Task<JoinTokenResponse> Join(JoinTokenRequest request, ServerCallContext context)
        {
            await _tokenService.JoinToken(request.Username, request.Token);
            return new JoinTokenResponse();
        }

        public override async Task<MatchStatusResponse> MatchStatus(MatchStatusRequest request, ServerCallContext context)
        {
            var matchId = await _tokenService.GetMatch(request.Username);
            return new MatchStatusResponse() { MatchId = matchId };
        }
    }
}
