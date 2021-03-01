using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RPSLS.SPA.Server.Config;
using RPSLS.SPA.Shared.Config;

namespace RPSLS.SPA.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly MultiplayerSettings _multiplayerSettings;
        private readonly TwitterOptions _twitterOptions;

        public SettingsController(IOptions<MultiplayerSettings> MultiplayerSettings, IOptions<TwitterOptions> TwitterOptions)
        {
            _multiplayerSettings = MultiplayerSettings.Value;
            _twitterOptions = TwitterOptions.Value;
        }

        public IActionResult Index()
        {
            var appSettings = new AppSettings
            {
                MultiplayerEnabled = _multiplayerSettings.Enabled,
                TwitterEnabled = !string.IsNullOrWhiteSpace(_twitterOptions.ConsumerKey) && !string.IsNullOrWhiteSpace(_twitterOptions.ConsumerSecret)
            };

            return Ok(appSettings);
        }
    }
}