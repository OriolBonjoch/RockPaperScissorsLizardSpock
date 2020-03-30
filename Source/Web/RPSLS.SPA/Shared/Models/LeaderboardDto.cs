using System.Collections.Generic;

namespace RPSLS.SPA.Shared.Models
{
    public class LeaderboardDto
    {
        public IEnumerable<LeaderboardEntryDto> Players { get; set; }
    }
}
