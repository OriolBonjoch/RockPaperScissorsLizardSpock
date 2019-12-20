namespace RPSLS.Game.Multiplayer.Config
{
    public class MultiplayerSettings
    {
        public string Title { get; set; }
        public string SecretKey { get; set; }

        public TokenSettings Token { get; set; } = new TokenSettings();
    }
}
