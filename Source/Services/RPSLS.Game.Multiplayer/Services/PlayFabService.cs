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
        private const string WinsStat = "Wins";
        private const string TotalStat = "Total";

        private readonly ILogger<PlayFabService> _logger;
        private readonly MultiplayerSettings _settings;

        public PlayFabService(ILogger<PlayFabService> logger, IOptions<MultiplayerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task Initialize()
        {
            PlayFabSettings.staticSettings.TitleId = _settings.Title;
            PlayFabSettings.staticSettings.DeveloperSecretKey = _settings.SecretKey;

            await EnsureQueueExist();
            await EnsureLeaderBoardExists();
        }

        public async Task<string> GetEntityToken(string userTitleId = null)
        {
            var tokenRequestBuilder = new GetEntityTokenRequestBuilder();
            if (!string.IsNullOrWhiteSpace(userTitleId))
            {
                tokenRequestBuilder.WithUserToken(userTitleId);
            }

            var entityTokenResult = await Call(
                PlayFabAuthenticationAPI.GetEntityTokenAsync,
                tokenRequestBuilder);
            return entityTokenResult.EntityToken;
        }

        public async Task<string> CreateTicket(string username, string token)
        {
            var userEntity = await GetUserEntity(username);

            await Call(
                PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayerAsync,
                new CancelAllMatchmakingTicketsForPlayerRequestBuilder()
                    .WithEntity(userEntity.Id, userEntity.Type)
                    .WithQueue(queueName));

            var ticketResult = await Call(
                PlayFabMultiplayerAPI.CreateMatchmakingTicketAsync,
                new CreateMatchmakingTicketRequestBuilder()
                    .WithCreatorEntity(userEntity.Id, userEntity.Type, token)
                    .WithGiveUpOf(120)
                    .WithQueue(queueName));

            return ticketResult.TicketId;
        }

        public async Task<MatchResult> CheckTicketStatus(string username, string ticketId)
        {
            var result = new MatchResult();
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                return result;
            }

            var userEntity = await GetUserEntity(username);
            var matchTicketResult = await Call(
                PlayFabMultiplayerAPI.GetMatchmakingTicketAsync,
                new GetMatchmakingTicketRequestBuilder()
                    .WithQueue(queueName)
                    .WithTicketId(ticketId));

            var status = matchTicketResult?.Status ?? string.Empty;
            result.Status = status;
            result.MatchId = matchTicketResult?.MatchId ?? string.Empty;
            if (result.Matched)
            {
                var getMatchResult = await Call(
                    PlayFabMultiplayerAPI.GetMatchAsync,
                    new GetMatchRequestBuilder()
                        .WithId(result.MatchId)
                        .WithQueue(queueName)
                        .WithMemberAttributes());

                var opponentEntity = getMatchResult.Members?.FirstOrDefault(u => u.Entity.Id != userEntity.Id);
                if (opponentEntity != null)
                {
                    result.Opponent = await GetUsername(opponentEntity.Entity.Id);
                }
            }

            return result;
        }

        public async Task UpdateStats(string username, bool isWinner)
        {
            var loginResult = await Call(
                PlayFabClientAPI.LoginWithCustomIDAsync,
                new LoginWithCustomIDRequestBuilder()
                    .WithUser(username)
                    .WithAccountInfo()
                    .CreateIfDoesntExist());

            var statsRequestBuilder = new UpdatePlayerStatisticsRequestBuilder()
                .WithPlayerId(loginResult.PlayFabId)
                .WithStatsIncrease(TotalStat);

            if (isWinner)
            {
                statsRequestBuilder.WithStatsIncrease(WinsStat);
            }

            await Call(PlayFabServerAPI.UpdatePlayerStatisticsAsync, statsRequestBuilder);
        }

        private async Task<EntityKey> GetUserEntity(string username)
        {
            var loginResult = await Call(
                PlayFabClientAPI.LoginWithCustomIDAsync,
                new LoginWithCustomIDRequestBuilder()
                    .WithUser(username)
                    .WithAccountInfo()
                    .CreateIfDoesntExist());

            var userEntity = loginResult.EntityToken.Entity;
            if (loginResult.NewlyCreated || loginResult.InfoResultPayload?.AccountInfo?.TitleInfo?.DisplayName != userEntity.Id)
            {
                // Add a DisplayName to the title user so its easier to retrieve the twitter user;
                await Call(
                    PlayFabClientAPI.UpdateUserTitleDisplayNameAsync,
                    new UpdateUserTitleDisplayNameRequestBuilder()
                        .WithName(userEntity.Id));
            }

            return userEntity;
        }

        private async Task<string> GetUsername(string userTitleId)
        {
            var getAccountResult = await Call(
                PlayFabAdminAPI.GetUserAccountInfoAsync,
                new LookupUserAccountInfoRequestBuilder()
                    .WithTitleDisplay(userTitleId));

            var username = getAccountResult.UserInfo.CustomIdInfo.CustomId;
            return username ?? "Unknown";
        }

        private async Task EnsureQueueExist()
        {
            var entityToken = await GetEntityToken();
            var fetchQueueResult = await Call(
                PlayFabMultiplayerAPI.GetMatchmakingQueueAsync,
                new GetMatchmakingQueueRequestBuilder()
                    .WithQueue(queueName)
                    .WithTitleContext(_settings.Title, entityToken));

            if (fetchQueueResult == null)
            {
                // Create if queue does not exist
                await Call(
                    PlayFabMultiplayerAPI.SetMatchmakingQueueAsync,
                    new SetMatchmakingQueueRequestBuilder()
                        .WithQueue(queueName, 2)
                        .WithTitleContext(_settings.Title, entityToken)
                        .WithQueueStringRule("TokenRule", "Token", "random"));
            }
        }

        private async Task EnsureLeaderBoardExists()
        {
            var statsResult = await Call(
                PlayFabAdminAPI.GetPlayerStatisticDefinitionsAsync,
                new GetPlayerStatisticDefinitionsRequestBuilder());

            if (statsResult?.Statistics?.FirstOrDefault(s => s.StatisticName == WinsStat) == null)
            {
                await Call(
                    PlayFabAdminAPI.CreatePlayerStatisticDefinitionAsync,
                    new CreatePlayerStatisticDefinitionRequestBuilder()
                        .WithAggregatedStat(WinsStat));
            }

            if (statsResult?.Statistics?.FirstOrDefault(s => s.StatisticName == TotalStat) == null)
            {
                await Call(
                    PlayFabAdminAPI.CreatePlayerStatisticDefinitionAsync,
                    new CreatePlayerStatisticDefinitionRequestBuilder()
                        .WithAggregatedStat(TotalStat));
            }
        }

        private async Task<U> Call<T, U>(Func<T, object, Dictionary<string, string>, Task<PlayFabResult<U>>> playFabCall, BaseRequestBuilder<T> requestBuilder)
            where U : PlayFabResultCommon
            where T : new()
            => (await CallWithError(playFabCall, requestBuilder)).Result;

        private async Task<PlayFabResult<U>> CallWithError<T, U>(Func<T, object, Dictionary<string, string>, Task<PlayFabResult<U>>> playFabCall, BaseRequestBuilder<T> requestBuilder)
            where U : PlayFabResultCommon
            where T : new()
        {
            var taskResult = await playFabCall(requestBuilder.Build(), null, null);
            var apiError = taskResult.Error;
            if (apiError != null)
            {
                var detailedError = PlayFabUtil.GenerateErrorReport(apiError);
                _logger.LogWarning($"Something went wrong with PlayFab API call {playFabCall.Method.Name}.{Environment.NewLine}{detailedError}");
            }

            return taskResult;
        }
    }
}
