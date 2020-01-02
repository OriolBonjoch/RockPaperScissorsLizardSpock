namespace RPSLS.Game.Multiplayer.Models
{
    public class MatchResult
    {
        public string Status { get; set; }
        public string MatchId { get; set; }
        public bool Finished { get; set; }
        public string TicketId { get; set; }
    }
}
