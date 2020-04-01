using Microsoft.Extensions.Options;
using RPSLS.SPA.Server.Clients;
using RPSLS.SPA.Shared.Config;

namespace RPSLS.SPA.Server.Config
{
    public class MultiplayerSettingsOptions : IConfigureOptions<MultiplayerSettings>
    {
        private readonly IConfigurationManagerClient _configurationManager;

        public MultiplayerSettingsOptions(IConfigurationManagerClient configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public void Configure(MultiplayerSettings options)
        {
            var gameApiSettingsClient = _configurationManager.GetSettings();
            options.Enabled = gameApiSettingsClient.HasMultiplayer;
        }
    }
  
}
