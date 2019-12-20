using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using RPSLS.Game.Multiplayer.Builders;
using RPSLS.Game.Multiplayer.Config;
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

        private string _entityToken;
        private DateTime? _expiration;

        public PlayFabService(ILogger<PlayFabService> logger, IOptions<MultiplayerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;

            PlayFabSettings.staticSettings.TitleId = _settings.Title;
            PlayFabSettings.staticSettings.DeveloperSecretKey = _settings.SecretKey;
        }

        protected string EntityToken
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_entityToken))
                    Initialize().RunSynchronously();

                return _entityToken;
            }
        }

        public async Task Initialize()
        {
            if (_expiration != null)
                if (_expiration.HasValue && _expiration.Value > DateTime.UtcNow) return;

            var entityTokenRequest = new GetEntityTokenRequestBuilder().Build();
            var entityTokenResult = await Call(PlayFabAuthenticationAPI.GetEntityTokenAsync, entityTokenRequest);
            _entityToken = entityTokenResult.EntityToken;
            _expiration = entityTokenResult.TokenExpiration;

            var validateTokenRequest = new ValidateEntityTokenRequestBuilder()
                .WithTitleContext(_settings.Title, EntityToken)
                .WithToken(EntityToken)
                .Build();

            await Call(PlayFabAuthenticationAPI.ValidateEntityTokenAsync, validateTokenRequest);
            _logger.LogInformation($"PlayFab {_settings.Title} token validated.");

            await EnsureQueueExist();
            await EnsureLeaderBoardExists();
        }

        public async Task<string> CreateTicket(string username, string token)
        {
            await Initialize();
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

        public async Task<string> CheckTicketStatus(string username)
        {
            await Initialize();
            var userEntity = await GetUserEntity(username);
            var listTicketsRequest = new ListMatchmakingTicketsForPlayerRequestBuilder()
                .WithQueue(queueName)
                .WithEntity(userEntity.Id, userEntity.Type)
                .Build();

            var listTickets = await Call(PlayFabMultiplayerAPI.ListMatchmakingTicketsForPlayerAsync, listTicketsRequest);
            var ticketId = listTickets.TicketIds.First();

            var awaitTicketRequest = new GetMatchmakingTicketRequestBuilder()
                .WithQueue(queueName)
                .WithTicketId(ticketId)
                .Build();

            var matchResult = await Call(PlayFabMultiplayerAPI.GetMatchmakingTicketAsync, awaitTicketRequest);
            return (matchResult?.Status != null && !matchResult.Status.StartsWith("Waiting")) ? matchResult.MatchId : null;
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
            if (fetchQueueResult != null)
            {
                // Create if queue does not exist
                var queueRequest = new SetMatchmakingQueueRequestBuilder()
                    .WithQueue(queueName, 2)
                    .WithTitleContext(_settings.Title, EntityToken)
                    .WithQueueStringRule("TokenRule", "Token")
                    .Build();

                await Call(PlayFabMultiplayerAPI.SetMatchmakingQueueAsync, queueRequest);
            }
        }

        private Task EnsureLeaderBoardExists() => Task.CompletedTask;

        private async Task<U> Call<T, U>(Func<T, object, Dictionary<string, string>, Task<PlayFabResult<U>>> playFabCall, T request) where U : PlayFabResultCommon
        {
            var taskResult = await playFabCall(request, null, null);
            var apiError = taskResult.Error;
            if (apiError != null)
            {
                var detailedError = PlayFabUtil.GenerateErrorReport(apiError);
                _logger.LogWarning($"Something went wrong with PlayFab API call.{Environment.NewLine}{detailedError}");
            }

            return taskResult.Result;
        }
    }
}
