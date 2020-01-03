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
    public class ResultsDao
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

        public async Task SaveMatch(PickDto pick, string username, int userPick, GameApi.Proto.Result result)
        {
            var dto = MatchDto.FromPickDto(pick);
            dto.PlayerName = username;
            dto.PlayerMove.Text = ToText(userPick);
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

        internal static string ToText(int userPick)
        {
            return userPick switch
            {
                0 => "Rock",
                1 => "Paper",
                2 => "Scissors",
                3 => "Lizard",
                4 => "Spock",
                _ => "Unknown",
            };
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
