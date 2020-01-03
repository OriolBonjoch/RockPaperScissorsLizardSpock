using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using RPSLS.Game.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPSLS.Game.Api.Data
{
    public class ResultsDao : IResultsDao
    {
        private const string DatabaseName = "rpsls";
        private readonly string _constr;
        private readonly ILogger<ResultsDao> _logger;


        public ResultsDao(string constr, ILoggerFactory loggerFactory)
        {
            _constr = constr;
            _logger = loggerFactory.CreateLogger<ResultsDao>();
        }

        public async Task CreateMatch(string matchId, string username, string challenger)
        {
            var dto = new MatchDto();
            dto.Challenger.Name = challenger;
            dto.Challenger.Type = "human";
            dto.PlayerName = username;
            dto.PlayFabMatchId = matchId;
            if (_constr == null)
            {
                _logger.LogInformation("+++ Cosmos constr is null. Doc that would be written is:");
                _logger.LogInformation(JsonSerializer.Serialize(dto));
                _logger.LogInformation("+++ Nothing was written on Cosmos");
                return;
            }

            var cResponse = await GetContainer();
            var response = await cResponse.Container.CreateItemAsync(dto);
            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation($"Cosmos save attempt resulted with StatusCode {response.StatusCode}.");
            }
        }

        public async Task<MatchDto> GetMatch(string matchId)
        {
            if (_constr == null) return CreateDummyMatch(matchId);
            var cResponse = await GetContainer();
            return GetMatch(cResponse, matchId);
        }

        public async Task<MatchDto> SaveMatchPick(string matchId, string username, int pick)
        {
            if (_constr == null) return CreateDummyMatch(matchId);
            var cResponse = await GetContainer();
            var dto = GetMatch(cResponse, matchId);
            if (dto.PlayerName == username)
            {
                dto.PlayerMove.Text = PickDto.ToText(pick);
                dto.PlayerMove.Value = pick;
            }
            else
            {
                dto.ChallengerMove.Text = PickDto.ToText(pick);
                dto.ChallengerMove.Value = pick;
            }

            var result = await cResponse.Container.UpsertItemAsync(dto);
            return result.Resource;
        }

        public async Task<MatchDto> SaveMatchResult(string matchId, GameApi.Proto.Result result)
        {
            if (_constr == null) return CreateDummyMatch(matchId);

            var cResponse = await GetContainer();
            var dto = GetMatch(cResponse, matchId);
            dto.Result.Value = (int)result;
            dto.Result.Winner = Enum.GetName(typeof(GameApi.Proto.Result), result);

            var response = await cResponse.Container.UpsertItemAsync(dto);
            return response.Resource;
        }

        public async Task SaveMatch(PickDto pick, string username, int userPick, GameApi.Proto.Result result)
        {
            var dto = MatchDto.FromPickDto(pick);
            dto.PlayerName = username;
            dto.PlayerMove.Text = PickDto.ToText(userPick);
            dto.PlayerMove.Value = userPick;
            dto.Result.Value = (int)result;
            dto.Result.Winner = Enum.GetName(typeof(GameApi.Proto.Result), result);

            if (_constr == null)
            {
                _logger.LogInformation("+++ Cosmos constr is null. Doc that would be written is:");
                _logger.LogInformation(JsonSerializer.Serialize(dto));
                _logger.LogInformation("+++ Nothing was written on Cosmos");
                return;
            }

            var cResponse = await GetContainer();
            var response = await cResponse.Container.CreateItemAsync(dto);
            if(response.StatusCode != System.Net.HttpStatusCode.OK &&  
                response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation($"Cosmos save attempt resulted with StatusCode {response.StatusCode}.");
            }
        }

        public async Task DeleteMatch(string matchId)
        {
            if (_constr == null) return;

            var cResponse = await GetContainer();
            var existing = cResponse.Container.GetItemLinqQueryable<MatchDto>()
                .Where(m => m.PlayFabMatchId == matchId)
                .FirstOrDefault();

            if (existing == null) return;
            await cResponse.Container.DeleteItemAsync<MatchDto>(matchId, new PartitionKey(existing.PlayerName));
        }

        public async Task<IEnumerable<MatchDto>> GetLastGamesOfPlayer(string player, int limit)
        {
            if (_constr == null)
            {
                _logger.LogInformation($"Cosmos constr is null. No games returned for player {player}.");
                return Enumerable.Empty<MatchDto>();
            }

            var cResponse = await GetContainer();
            var sqlQueryText = $"SELECT * FROM g WHERE g.playerName = '{player}' AND (NOT(IS_DEFINED(g.playFabMatchId)) OR IS_NULL(g.playFabMatchId)) ORDER BY g.whenUtc DESC";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var rs = cResponse.Container.GetItemQueryIterator<MatchDto>(queryDefinition);
            var results = new List<MatchDto>();
            while (rs.HasMoreResults && (limit <= 0 || results.Count < limit))
            {
                var items = await rs.ReadNextAsync();
                results.AddRange(items);
            }

            return limit > 0 ? results.Take(limit).ToList() : results.ToList();
        }

        private MatchDto CreateDummyMatch(string matchId)
        {
            _logger.LogInformation("+++ Cosmos constr is null. No multiplayer game found, returning dummy game");
            var dto = new MatchDto();
            dto.Challenger.Name = "dummy";
            dto.Challenger.Type = "human";
            dto.PlayFabMatchId = matchId;
            return dto;
        }

        private static MatchDto GetMatch(ContainerResponse cResponse, string matchId)
        {
            return cResponse.Container.GetItemLinqQueryable<MatchDto>()
                .Where(m => m.PlayFabMatchId == matchId)
                .FirstOrDefault();
        }

        private async Task<ContainerResponse> GetContainer()
        {
            var client = new CosmosClient(_constr);
            var db = client.GetDatabase(DatabaseName);
            var cprops = new ContainerProperties()
            {
                Id = "results",
                PartitionKeyPath = "/playerName"
            };
            return await db.CreateContainerIfNotExistsAsync(cprops);
        }
    }
}
