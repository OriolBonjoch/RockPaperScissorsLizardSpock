using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Services;
using System.Threading.Tasks;
using TokenApi.Proto;

namespace RPSLS.Game.Api.GrpcServices
{
    public class TokenManagerService : TokenManager.TokenManagerBase
    {
        private const int FREE_TIER_MAX_REQUESTS = 10;
        private readonly IPlayFabService _playFabService;
        private readonly ITokenService _tokenService;
        private readonly TokenSettings _tokenSettings;
        private readonly ILogger<TokenManagerService> _logger;

        public TokenManagerService(
            IPlayFabService playFabService,
            ITokenService tokenService,
            IOptions<TokenSettings> options,
            ILogger<TokenManagerService> logger)
        {
            _playFabService = playFabService;
            _tokenService = tokenService;
            _tokenSettings = options.Value;
            _logger = logger;

            if (_tokenSettings.TicketStatusWait < 60000 / FREE_TIER_MAX_REQUESTS)
            {
                _logger.LogWarning($"PlayFab free tier limits the Get Matchmaking Ticket requests to a max of {FREE_TIER_MAX_REQUESTS} per minute. " +
                    $"A MatchmakingRateLimitExceeded error might occur while waiting for a multiplayer match");
            }
        }

        public override async Task<CreateTokenResponse> CreateToken(CreateTokenRequest request, ServerCallContext context)
        {
            await _playFabService.Initialize();
            var token = await _tokenService.CreateToken(request.Username);
            _logger.LogInformation($"New token created for user {request.Username}: {token}");
            return new CreateTokenResponse() { Token = token };
        }

        public override async Task<JoinTokenResponse> Join(JoinTokenRequest request, ServerCallContext context)
        {
            await _playFabService.Initialize();
            await _tokenService.JoinToken(request.Username, request.Token);
            return new JoinTokenResponse();
        }

        public override async Task WaitMatch(MatchStatusRequest request, IServerStreamWriter<MatchStatusResponse> responseStream, ServerCallContext context)
        {
            await _playFabService.Initialize();
            var matchResult = await _tokenService.GetMatch(request.Username);
            var response = new MatchStatusResponse() { MatchId = string.Empty, Status = matchResult.Status };
            while (!matchResult.Finished)
            {
                await responseStream.WriteAsync(response);
                await Task.Delay(_tokenSettings.TicketStatusWait);
                matchResult = await _tokenService.GetMatch(request.Username, matchResult.TicketId);
                response = new MatchStatusResponse() { MatchId = string.Empty, Status = matchResult.Status };
            }

            await responseStream.WriteAsync(new MatchStatusResponse() {
                MatchId = matchResult.MatchId ?? string.Empty,
                Status = matchResult.Status
            });
        }
    }
}
