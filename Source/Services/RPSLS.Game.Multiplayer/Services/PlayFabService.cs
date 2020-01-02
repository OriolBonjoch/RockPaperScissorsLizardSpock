using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using RPSLS.Game.Multiplayer.Builders;
using RPSLS.Game.Multiplayer.Config;
using RPSLS.Game.Multiplayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPSLS.Game.Multiplayer.Services
{
    public class PlayFabService : IPlayFabService
    {
        private const string queueName = "rpsls_queue";
        private readonly ILogger<PlayFabService> _logger;
        private readonly MultiplayerSettings _settings;
        private DateTime? _expiration;

        public PlayFabService(ILogger<PlayFabService> logger, IOptions<MultiplayerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task Initialize()
        {
            PlayFabSettings.staticSettings.TitleId = _settings.Title;
            PlayFabSettings.staticSettings.DeveloperSecretKey = _settings.SecretKey;

            await ValidateToken();
            await EnsureQueueExist();
            await EnsureLeaderBoardExists();
        }

        protected string EntityToken { get; private set; }

        public async Task ValidateToken()
        {
            if (_expiration != null)
                if (_expiration.HasValue && _expiration.Value > DateTime.UtcNow) return;
            
            if (!string.IsNullOrWhiteSpace(EntityToken))
                return;

            var entityTokenRequest = new GetEntityTokenRequestBuilder().Build();
            var entityTokenResult = await Call(PlayFabAuthenticationAPI.GetEntityTokenAsync, entityTokenRequest);
            EntityToken = entityTokenResult.EntityToken;
            _expiration = entityTokenResult.TokenExpiration;

            var validateTokenRequest = new ValidateEntityTokenRequestBuilder()
                .WithTitleContext(_settings.Title, EntityToken)
                .WithToken(EntityToken)
                .Build();

            await Call(PlayFabAuthenticationAPI.ValidateEntityTokenAsync, validateTokenRequest);
            _logger.LogInformation($"PlayFab {_settings.Title} token validated.");
        }

        public async Task<string> CreateTicket(string username, string token)
        {
            await ValidateToken();
            var userEntity = await GetUserEntity(username);
            var cancelRequest = new CancelAllMatchmakingTicketsForPlayerRequestBuilder()
                .WithEntity(userEntity.Id, userEntity.Type)
                .WithQueue(queueName)
                .Build();
            await Call(PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayerAsync, cancelRequest);

            // Create Matchmaking ticket
            var matchRequest = new CreateMatchmakingTicketRequestBuilder()
                .WithCreatorEntity(userEntity.Id, userEntity.Type, token)
                .WithGiveUpOf(120)
                .WithQueue(queueName)
                .Build();

            var ticketResult = await Call(PlayFabMultiplayerAPI.CreateMatchmakingTicketAsync, matchRequest);
            return ticketResult.TicketId;
        }

        public async Task<MatchResult> CheckTicketStatus(string username, string ticketId = null)
        {
            await ValidateToken();
            var result = new MatchResult() { TicketId = ticketId ?? string.Empty };
            var userEntity = await GetUserEntity(username);
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                var listTicketsRequest = new ListMatchmakingTicketsForPlayerRequestBuilder()
                    .WithEntity(userEntity.Id, userEntity.Type)
                    .WithQueue(queueName)
                    .Build();

                var listTickets = await Call(PlayFabMultiplayerAPI.ListMatchmakingTicketsForPlayerAsync, listTicketsRequest);
                result.TicketId = listTickets?.TicketIds?.FirstOrDefault();
            }
            
            if (string.IsNullOrWhiteSpace(result.TicketId)) return result;

            var awaitTicketRequest = new GetMatchmakingTicketRequestBuilder()
                .WithUserContext(userEntity.Id, EntityToken)
                .WithQueue(queueName)
                .WithTicketId(result.TicketId)
                .Build();

            var matchResult = await Call(PlayFabMultiplayerAPI.GetMatchmakingTicketAsync, awaitTicketRequest);
            result.Status = matchResult?.Status ?? string.Empty;
            result.MatchId = matchResult?.MatchId ?? string.Empty;
            result.Finished = matchResult?.Status != null && !matchResult.Status.StartsWith("Waiting");
            return result;
        }

        private async Task<EntityKey> GetUserEntity(string username)
        {
            var loginRequest = new LoginWithCustomIDRequestBuilder()
                .WithUser(username)
                .CreateIfDoesntExist()
                .Build();

            var loginResult = await Call(PlayFabClientAPI.LoginWithCustomIDAsync, loginRequest);
            var userEntity = loginResult.EntityToken.Entity;
            return userEntity;
        }

        private async Task EnsureQueueExist()
        {
            var fetchQueueRequest = new GetMatchmakingQueueRequestBuilder()
                .WithQueue(queueName)
                .WithTitleContext(_settings.Title, EntityToken)
                .Build();

            var fetchQueueResult = await Call(PlayFabMultiplayerAPI.GetMatchmakingQueueAsync, fetchQueueRequest);
            if (fetchQueueResult == null)
            {
                // Create if queue does not exist
                var queueRequest = new SetMatchmakingQueueRequestBuilder()
                    .WithQueue(queueName, 2)
                    .WithTitleContext(_settings.Title, EntityToken)
                    .WithQueueStringRule("TokenRule", "Token", "random")
                    .Build();

                await Call(PlayFabMultiplayerAPI.SetMatchmakingQueueAsync, queueRequest);
            }
        }

        private Task EnsureLeaderBoardExists() => Task.CompletedTask;

        private async Task<U> Call<T, U>(Func<T, object, Dictionary<string, string>, Task<PlayFabResult<U>>> playFabCall, T request) where U : PlayFabResultCommon
            => (await CallWithError(playFabCall, request)).Result;

        private async Task<PlayFabResult<U>> CallWithError<T, U>(Func<T, object, Dictionary<string, string>, Task<PlayFabResult<U>>> playFabCall, T request) where U : PlayFabResultCommon
        {
            var taskResult = await playFabCall(request, null, null);
            var apiError = taskResult.Error;
            if (apiError != null)
            {
                var detailedError = PlayFabUtil.GenerateErrorReport(apiError);
                _logger.LogWarning($"Something went wrong with PlayFab API call.{Environment.NewLine}{detailedError}");
            }

            return taskResult;
        }
    }
}
