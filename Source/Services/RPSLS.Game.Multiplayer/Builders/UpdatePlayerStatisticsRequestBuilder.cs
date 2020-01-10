using PlayFab.ClientModels;
using System.Collections.Generic;

namespace RPSLS.Game.Multiplayer.Builders
{
    public class UpdatePlayerStatisticsRequestBuilder : PlayFabRequestCommonBuilder<UpdatePlayerStatisticsRequestBuilder, UpdatePlayerStatisticsRequest>
    {
        public UpdatePlayerStatisticsRequestBuilder WithStatsIncrease(string name)
        {
            if (_product.Statistics == null)
            {
                _product.Statistics = new List<StatisticUpdate>();
            }

            var stats = new StatisticUpdate()
            {
                StatisticName = name,
                Value = 1
            };

            _product.Statistics.Add(stats);
            return this;
        }
    }
}
