using GameApi.Proto;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPSLS.Game.Api.Data;
using RPSLS.Game.Api.Services;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Services;
using System;
using System.Threading.Tasks;

namespace RPSLS.Game.Api.GrpcServices
{
    public class MultiplayerGameManagerService : MultiplayerGameManager.MultiplayerGameManagerBase
    {
        private const int FREE_TIER_MAX_REQUESTS = 10;
        private readonly GameStatusResponse _cancelledMatch = new GameStatusResponse { IsCancelled = true };

        private readonly IPlayFabService _playFabService;
        private readonly ITokenService _tokenService;
        private readonly IGameService _gameService;
        private readonly IResultsDao _resultsDao;
        private readonly MultiplayerSettings _multiplayerSettings;
        private readonly ILogger<MultiplayerGameManagerService> _logger;

        public MultiplayerGameManagerService(
            IPlayFabService playFabService,
            ITokenService tokenService,
            IGameService gameService,
            IOptions<MultiplayerSettings> options,
            IResultsDao resultsDao,
            ILogger<MultiplayerGameManagerService> logger)
        {
            _playFabService = playFabService;
            _tokenService = tokenService;
            _gameService = gameService;
            _resultsDao = resultsDao;
            _multiplayerSettings = options.Value;
            _logger = logger;

            if (_multiplayerSettings.Token.TicketStatusWait < 60000 / FREE_TIER_MAX_REQUESTS)
            {
                _logger.LogWarning($"PlayFab free tier limits the Get Matchmaking Ticket requests to a max of {FREE_TIER_MAX_REQUESTS} per minute. " +
                    "A MatchmakingRateLimitExceeded error might occur while waiting for a multiplayer match");
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
            if (string.IsNullOrWhiteSpace(matchResult.TicketId))
            {
                await responseStream.WriteAsync(CreateMatchStatusResponse("RateLimitExceeded"));
            }

            while (!matchResult.Finished && !context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(CreateMatchStatusResponse(matchResult.Status));
                await Task.Delay(_multiplayerSettings.Token.TicketStatusWait);
                matchResult = await _tokenService.GetMatch(username, matchResult.TicketId);
            }

            await responseStream.WriteAsync(CreateMatchStatusResponse(matchResult.Status, matchResult.MatchId));
            if (request.IsMaster)
            {
                await _resultsDao.CreateMatch(matchResult.MatchId, username, matchResult.Opponent);
            }
        }

        public override async Task GameStatus(GameStatusRequest request, IServerStreamWriter<GameStatusResponse> responseStream, ServerCallContext context)
        {
            var dto = await _resultsDao.GetMatch(request.MatchId);
            while (dto == null)
            {
                await Task.Delay(_multiplayerSettings.GameStatusUpdateDelay);
                dto = await _resultsDao.GetMatch(request.MatchId);
            }

            var isMaster = dto.PlayerName == request.Username;
            var result = isMaster ? CreateGameStatusForMaster(dto) : CreateGameStatusForOpponent(dto);
            await responseStream.WriteAsync(result);
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_multiplayerSettings.GameStatusUpdateDelay);
                dto = await _resultsDao.GetMatch(request.MatchId);
                if (dto == null)
                {
                    await responseStream.WriteAsync(_cancelledMatch);
                    return;
                }

                var matchExpired = DateTime.UtcNow.AddSeconds(-_multiplayerSettings.GameStatusMaxWait) < dto.WhenUtc;
                if (isMaster && matchExpired)
                {
                    await _resultsDao.DeleteMatch(request.MatchId);
                    await responseStream.WriteAsync(_cancelledMatch);
                    return;
                }

                result = isMaster ? CreateGameStatusForMaster(dto) : CreateGameStatusForOpponent(dto);
                await responseStream.WriteAsync(result);
            }
        }

        public override async Task<Empty> Pick(PickRequest request, ServerCallContext context)
        {
            var dto = await _resultsDao.SaveMatchPick(request.MatchId, request.Username, request.Pick);
            if (!string.IsNullOrWhiteSpace(dto.ChallengerMove?.Text) && !string.IsNullOrWhiteSpace(dto.PlayerMove?.Text))
            {
                var result = _gameService.Check(dto.PlayerMove.Value, dto.ChallengerMove.Value);
                await _resultsDao.SaveMatchResult(request.MatchId, result);
            }

            return new Empty();
        }

        private static PairingStatusResponse CreateMatchStatusResponse(string status, string matchId = null)
            => new PairingStatusResponse()
            {
                Status = status ?? string.Empty,
                MatchId = matchId ?? string.Empty,
            };

        private static GameStatusResponse CreateGameStatusForMaster(MatchDto match)
        {
            return new GameStatusResponse
            {
                User = match.PlayerName,
                UserPick = match.PlayerMove.Value,
                Challenger = match.Challenger.Name,
                ChallengerPick = match.ChallengerMove.Value,
                Result = (Result)match.Result.Value,
                IsMaster = true,
                IsCancelled = false,
                IsFinished = !string.IsNullOrWhiteSpace(match.Result.Winner)
            };
        }

        private static GameStatusResponse CreateGameStatusForOpponent(MatchDto match)
        {
            return new GameStatusResponse
            {
                User = match.Challenger.Name,
                UserPick = match.ChallengerMove.Value,
                Challenger = match.PlayerName,
                ChallengerPick = match.PlayerMove.Value,
                Result = (Result)match.Result.Value,
                IsMaster = false,
                IsCancelled = false,
                IsFinished = !string.IsNullOrWhiteSpace(match.Result.Winner)
            };
        }
    }
}
