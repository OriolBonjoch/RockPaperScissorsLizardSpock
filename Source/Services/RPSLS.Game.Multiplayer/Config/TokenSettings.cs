namespace RPSLS.Game.Multiplayer.Config
{
    public class TokenSettings
    {
        public int Length { get; set; } = 5;
        public string ValidCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public int TicketStatusWait { get; set; } = 6000;
    }
}
