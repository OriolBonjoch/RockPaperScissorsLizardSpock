using GameApi.Proto;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPSLS.Game.Api.Data;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Services;
using System.Threading.Tasks;

namespace RPSLS.Game.Api.GrpcServices
{
    public class MultiplayerGameManagerService : MultiplayerGameManager.MultiplayerGameManagerBase
    {
        private const int FREE_TIER_MAX_REQUESTS = 10;
        private readonly IPlayFabService _playFabService;
        private readonly ITokenService _tokenService;
        private readonly ResultsDao _resultsDao;
        private readonly TokenSettings _tokenSettings;
        private readonly ILogger<MultiplayerGameManagerService> _logger;

        public MultiplayerGameManagerService(
            IPlayFabService playFabService,
            ITokenService tokenService,
            IOptions<TokenSettings> options,
            ResultsDao resultsDao,
            ILogger<MultiplayerGameManagerService> logger)
        {
            _playFabService = playFabService;
            _tokenService = tokenService;
            _resultsDao = resultsDao;
            _tokenSettings = options.Value;
            _logger = logger;

            if (_tokenSettings.TicketStatusWait < 60000 / FREE_TIER_MAX_REQUESTS)
            {
                _logger.LogWarning($"PlayFab free tier limits the Get Matchmaking Ticket requests to a max of {FREE_TIER_MAX_REQUESTS} per minute. " +
                    $"A MatchmakingRateLimitExceeded error might occur while waiting for a multiplayer match");
            }
        }

        public override async Task<CreatePairingResponse> CreatePairing(CreatePairingRequest request, ServerCallContext context)
        {
            await _playFabService.Initialize();
            var token = await _tokenService.CreateToken(request.Username);
            _logger.LogInformation($"New token created for user {request.Username}: {token}");
            return new CreatePairingResponse() { Token = token };
        }

        public override async Task<Empty> JoinPairing(JoinPairingRequest request, ServerCallContext context)
        {
            await _playFabService.Initialize();
            await _tokenService.JoinToken(request.Username, request.Token);
            return new Empty();
        }

        public override async Task PairingStatus(PairingStatusRequest request, IServerStreamWriter<PairingStatusResponse> responseStream, ServerCallContext context)
        {
            await _playFabService.Initialize();
            var username = request.Username;
            var matchResult = await _tokenService.GetMatch(username);
            while (!matchResult.Finished && !context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(CreateMatchStatusResponse(matchResult.Status));
                await Task.Delay(_tokenSettings.TicketStatusWait);
                matchResult = await _tokenService.GetMatch(username, matchResult.TicketId);
            }

            await responseStream.WriteAsync(CreateMatchStatusResponse(matchResult.Status, matchResult.MatchId));
            if (request.IsMaster)
            {
                await _resultsDao.CreateMatch(matchResult.MatchId, username, matchResult.Opponent);
            }
        }

        private static PairingStatusResponse CreateMatchStatusResponse(string status, string matchId = null)
            => new PairingStatusResponse()
            {
                Status = status ?? string.Empty,
                MatchId = matchId ?? string.Empty,
            };
    }
}
